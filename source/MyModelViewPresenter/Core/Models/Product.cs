using System;
using System.ComponentModel.DataAnnotations;

namespace Core.Models
{
    /// <summary>
    /// Product entity representing a product in the system.
    /// Follows Domain-Driven Design principles with proper encapsulation and validation.
    /// </summary>
    public class Product
    {
        /// <summary>
        /// Unique identifier for the product.
        /// </summary>
        public int Id { get; set; }
        
        /// <summary>
        /// Name of the product.
        /// </summary>
        [Required(ErrorMessage = "Product name is required")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Product name must be between 2 and 100 characters")]
        public string Name { get; set; }
        
        /// <summary>
        /// Price of the product in the system's base currency.
        /// </summary>
        [Required(ErrorMessage = "Price is required")]
        [Range(0.01, 999999.99, ErrorMessage = "Price must be between $0.01 and $999,999.99")]
        [DataType(DataType.Currency)]
        public decimal Price { get; set; }
        
        /// <summary>
        /// Optional description of the product.
        /// </summary>
        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        [DataType(DataType.MultilineText)]
        public string Description { get; set; }
        
        /// <summary>
        /// Current stock quantity available.
        /// </summary>
        [Required(ErrorMessage = "Stock quantity is required")]
        [Range(0, int.MaxValue, ErrorMessage = "Stock quantity cannot be negative")]
        [Display(Name = "Stock Quantity")]
        public int StockQuantity { get; set; }
        
        /// <summary>
        /// Date and time when the product was created.
        /// </summary>
        [Display(Name = "Created Date")]
        [DataType(DataType.DateTime)]
        public DateTime CreatedDate { get; set; }
        
        /// <summary>
        /// Date and time when the product was last modified.
        /// </summary>
        [Display(Name = "Modified Date")]
        [DataType(DataType.DateTime)]
        public DateTime? ModifiedDate { get; set; }
        
        /// <summary>
        /// Indicates whether the product is active (not deleted).
        /// </summary>
        [Display(Name = "Is Active")]
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Calculated property to determine if the product is out of stock.
        /// </summary>
        public bool IsOutOfStock => StockQuantity <= 0;

        /// <summary>
        /// Calculated property to determine if the product is low in stock.
        /// </summary>
        public bool IsLowStock => StockQuantity > 0 && StockQuantity <= 5;

        /// <summary>
        /// Returns a string representation of the product.
        /// </summary>
        public override string ToString()
        {
            return $"{Name} (${Price:F2}) - Stock: {StockQuantity}";
        }

        /// <summary>
        /// Determines equality based on the product ID.
        /// </summary>
        public override bool Equals(object obj)
        {
            if (obj is Product other)
            {
                return Id == other.Id && Id > 0;
            }
            return false;
        }

        /// <summary>
        /// Returns the hash code based on the product ID.
        /// </summary>
        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        /// <summary>
        /// Creates a deep copy of the product.
        /// </summary>
        public Product Clone()
        {
            return new Product
            {
                Id = this.Id,
                Name = this.Name,
                Price = this.Price,
                Description = this.Description,
                StockQuantity = this.StockQuantity,
                CreatedDate = this.CreatedDate,
                ModifiedDate = this.ModifiedDate,
                IsActive = this.IsActive
            };
        }
    }
}