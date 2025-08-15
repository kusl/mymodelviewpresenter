using Microsoft.Extensions.DependencyInjection;
using ProductApp.Domain;
using ProductApp.Infrastructure;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ProductApp.Application
{
    // Validation abstraction (OCP)
    public interface IValidator<T>
    {
        Dictionary<string, List<string>> Validate(T item);
    }

    // Product-specific validator
    public class ProductValidator : IValidator<CreateProductDto>
    {
        public Dictionary<string, List<string>> Validate(CreateProductDto item)
        {
            var errors = new Dictionary<string, List<string>>();
            var validationResults = new List<ValidationResult>();
            var context = new ValidationContext(item);

            if (!Validator.TryValidateObject(item, context, validationResults, true))
            {
                foreach (var result in validationResults)
                {
                    var memberName = result.MemberNames.FirstOrDefault() ?? "General";
                    if (!errors.ContainsKey(memberName))
                        errors[memberName] = new List<string>();
                    errors[memberName].Add(result.ErrorMessage ?? "Validation error");
                }
            }

            // Custom business rule validations
            if (item.Price <= 0)
            {
                if (!errors.ContainsKey(nameof(item.Price)))
                    errors[nameof(item.Price)] = new List<string>();
                errors[nameof(item.Price)].Add("Price must be greater than zero");
            }

            return errors;
        }
    }

    // Mapper abstraction for DTO conversions
    public interface IMapper<TSource, TDestination>
    {
        TDestination Map(TSource source);
        List<TDestination> MapList(List<TSource> source);
    }

    public class ProductMapper : IMapper<Product, ProductDto>
    {
        public ProductDto Map(Product source)
        {
            return new ProductDto
            {
                Id = source.Id,
                Name = source.Name,
                Price = source.Price,
                Description = source.Description,
                StockQuantity = source.StockQuantity,
                CreatedDate = source.CreatedDate,
                ModifiedDate = source.ModifiedDate
            };
        }

        public List<ProductDto> MapList(List<Product> source)
        {
            return source.Select(Map).ToList();
        }
    }

    // Service interface
    public interface IProductService
    {
        Task<Result<List<ProductDto>>> GetAllProductsAsync();
        Task<Result<ProductDto>> GetProductByIdAsync(int id);
        Task<Result<List<ProductDto>>> SearchProductsAsync(string term);
        Task<Result<ProductDto>> CreateProductAsync(CreateProductDto dto);
        Task<Result<ProductDto>> UpdateProductAsync(UpdateProductDto dto);
        Task<Result<bool>> DeleteProductAsync(int id);
    }

    // Base service for common operations (DRY)
    public abstract class BaseService
    {
        protected Result<T> HandleException<T>(Exception ex, string operation)
        {
            // Log exception here
            return Result<T>.Failure($"Error during {operation}: {ex.Message}");
        }
    }

    // Service implementation
    public class ProductService : BaseService, IProductService
    {
        private readonly IProductRepository _repository;
        private readonly IValidator<CreateProductDto> _validator;
        private readonly IMapper<Product, ProductDto> _mapper;

        public ProductService(
            IProductRepository repository,
            IValidator<CreateProductDto> validator,
            IMapper<Product, ProductDto> mapper)
        {
            _repository = repository;
            _validator = validator;
            _mapper = mapper;
        }

        public async Task<Result<List<ProductDto>>> GetAllProductsAsync()
        {
            try
            {
                var products = await _repository.GetAllAsync();
                var dtos = _mapper.MapList(products);
                return Result<List<ProductDto>>.Success(dtos);
            }
            catch (Exception ex)
            {
                return HandleException<List<ProductDto>>(ex, "retrieving products");
            }
        }

        public async Task<Result<ProductDto>> GetProductByIdAsync(int id)
        {
            try
            {
                var product = await _repository.GetByIdAsync(id);
                if (product == null)
                    return Result<ProductDto>.Failure("Product not found");

                var dto = _mapper.Map(product);
                return Result<ProductDto>.Success(dto);
            }
            catch (Exception ex)
            {
                return HandleException<ProductDto>(ex, "retrieving product");
            }
        }

        public async Task<Result<List<ProductDto>>> SearchProductsAsync(string term)
        {
            try
            {
                var products = await _repository.SearchAsync(term);
                var dtos = _mapper.MapList(products);
                return Result<List<ProductDto>>.Success(dtos);
            }
            catch (Exception ex)
            {
                return HandleException<List<ProductDto>>(ex, "searching products");
            }
        }

        public async Task<Result<ProductDto>> CreateProductAsync(CreateProductDto dto)
        {
            try
            {
                // Validate
                var validationErrors = _validator.Validate(dto);
                if (validationErrors.Any())
                    return Result<ProductDto>.ValidationFailure(validationErrors);

                // Check for duplicates
                var nameSpec = new ProductByNameSpecification(dto.Name);
                if (await _repository.ExistsAsync(nameSpec))
                    return Result<ProductDto>.Failure("A product with this name already exists");

                // Create entity
                var product = new Product
                {
                    Name = dto.Name,
                    Price = dto.Price,
                    Description = dto.Description,
                    StockQuantity = dto.StockQuantity
                };

                var created = await _repository.AddAsync(product);
                var resultDto = _mapper.Map(created);
                return Result<ProductDto>.Success(resultDto);
            }
            catch (Exception ex)
            {
                return HandleException<ProductDto>(ex, "creating product");
            }
        }

        public async Task<Result<ProductDto>> UpdateProductAsync(UpdateProductDto dto)
        {
            try
            {
                // Validate
                var validationErrors = _validator.Validate(dto);
                if (validationErrors.Any())
                    return Result<ProductDto>.ValidationFailure(validationErrors);

                // Get existing
                var existing = await _repository.GetByIdAsync(dto.Id);
                if (existing == null)
                    return Result<ProductDto>.Failure("Product not found");

                // Check for duplicate names (excluding current product)
                var products = await _repository.FindAsync(
                    new ProductByNameSpecification(dto.Name));
                if (products.Any(p => p.Id != dto.Id))
                    return Result<ProductDto>.Failure("A product with this name already exists");

                // Update entity
                existing.Name = dto.Name;
                existing.Price = dto.Price;
                existing.Description = dto.Description;
                existing.UpdateStock(dto.StockQuantity);

                var updated = await _repository.UpdateAsync(existing);
                var resultDto = _mapper.Map(updated);
                return Result<ProductDto>.Success(resultDto);
            }
            catch (Exception ex)
            {
                return HandleException<ProductDto>(ex, "updating product");
            }
        }

        public async Task<Result<bool>> DeleteProductAsync(int id)
        {
            try
            {
                var result = await _repository.DeleteAsync(id);
                if (!result)
                    return Result<bool>.Failure("Product not found");

                return Result<bool>.Success(true);
            }
            catch (Exception ex)
            {
                return HandleException<bool>(ex, "deleting product");
            }
        }
    }

    // Service factory for dependency injection setup
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            services.AddScoped<IValidator<CreateProductDto>, ProductValidator>();
            services.AddScoped<IMapper<Product, ProductDto>, ProductMapper>();
            services.AddScoped<IProductService, ProductService>();
            return services;
        }
    }
}