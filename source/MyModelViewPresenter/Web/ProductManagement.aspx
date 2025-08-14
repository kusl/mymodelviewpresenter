<%@ Page Title="Product Management" Language="C#" MasterPageFile="~/Site.Master" 
    AutoEventWireup="true" CodeBehind="ProductManagement.aspx.cs" 
    Inherits="Web.ProductManagement" Async="true" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style>
        /* Page Header */
        .page-header {
            background: rgba(255, 255, 255, 0.95);
            backdrop-filter: blur(20px);
            -webkit-backdrop-filter: blur(20px);
            border-radius: var(--border-radius-xl);
            padding: 2.5rem;
            margin-bottom: 2rem;
            box-shadow: var(--shadow-lg);
            border: 1px solid rgba(255, 255, 255, 0.2);
            position: relative;
            overflow: hidden;
        }

        [data-theme="dark"] .page-header {
            background: rgba(255, 255, 255, 0.03);
            border: 1px solid rgba(255, 255, 255, 0.1);
        }

        .page-header::before {
            content: '';
            position: absolute;
            top: 0;
            left: 0;
            right: 0;
            height: 4px;
            background: var(--primary-gradient);
        }

        .page-title {
            font-size: 2.5rem;
            font-weight: 800;
            background: var(--primary-gradient);
            -webkit-background-clip: text;
            -webkit-text-fill-color: transparent;
            background-clip: text;
            margin: 0;
            display: flex;
            align-items: center;
            gap: 1rem;
        }

        .page-title i {
            font-size: 2rem;
        }

        /* Search Section */
        .search-section {
            background: rgba(255, 255, 255, 0.95);
            backdrop-filter: blur(10px);
            -webkit-backdrop-filter: blur(10px);
            border-radius: var(--border-radius-lg);
            padding: 1.5rem;
            margin-bottom: 2rem;
            box-shadow: var(--shadow-md);
            border: 1px solid rgba(255, 255, 255, 0.2);
        }

        [data-theme="dark"] .search-section {
            background: rgba(255, 255, 255, 0.03);
            border: 1px solid rgba(255, 255, 255, 0.1);
        }

        .search-input-group {
            position: relative;
        }

        .search-input-group .form-control {
            padding-left: 3rem;
            height: 50px;
            font-size: 1rem;
            border: 2px solid rgba(102, 126, 234, 0.2);
            transition: all 0.3s ease;
        }

        .search-input-group .form-control:focus {
            border-color: var(--bs-primary);
            box-shadow: 0 0 0 0.25rem rgba(102, 126, 234, 0.25);
            transform: translateY(-2px);
        }

        .search-input-group::before {
            content: '\F52A';
            font-family: 'bootstrap-icons';
            position: absolute;
            left: 1rem;
            top: 50%;
            transform: translateY(-50%);
            font-size: 1.25rem;
            color: var(--bs-primary);
            z-index: 10;
        }

        /* Enhanced Alerts */
        .alert-animated {
            animation: slideInDown 0.5s ease;
            position: relative;
            padding-left: 4rem;
        }

        .alert-animated::before {
            content: '';
            position: absolute;
            left: 1.5rem;
            top: 50%;
            transform: translateY(-50%);
            width: 2rem;
            height: 2rem;
            border-radius: 50%;
            display: flex;
            align-items: center;
            justify-content: center;
        }

        .alert-success.alert-animated::before {
            content: '\F26B';
            font-family: 'bootstrap-icons';
            background: var(--success-gradient);
            color: white;
            font-size: 1rem;
            padding: 0.5rem;
        }

        .alert-danger.alert-animated::before {
            content: '\F336';
            font-family: 'bootstrap-icons';
            background: linear-gradient(135deg, #f5576c 0%, #f093fb 100%);
            color: white;
            font-size: 1rem;
            padding: 0.5rem;
        }

        @keyframes slideInDown {
            from {
                opacity: 0;
                transform: translateY(-20px);
            }
            to {
                opacity: 1;
                transform: translateY(0);
            }
        }

        /* Product Form Card */
        .form-card {
            background: rgba(255, 255, 255, 0.95);
            backdrop-filter: blur(20px);
            -webkit-backdrop-filter: blur(20px);
            border: 1px solid rgba(255, 255, 255, 0.2);
            border-radius: var(--border-radius-lg);
            overflow: hidden;
            box-shadow: var(--shadow-xl);
            margin-bottom: 2rem;
            animation: expandIn 0.5s ease;
        }

        [data-theme="dark"] .form-card {
            background: rgba(255, 255, 255, 0.03);
            border: 1px solid rgba(255, 255, 255, 0.1);
        }

        @keyframes expandIn {
            from {
                opacity: 0;
                transform: scale(0.95);
            }
            to {
                opacity: 1;
                transform: scale(1);
            }
        }

        .form-card .card-header {
            background: var(--primary-gradient);
            color: white;
            padding: 1.5rem;
            border: none;
        }

        .form-card .card-header h4 {
            margin: 0;
            font-weight: 600;
            display: flex;
            align-items: center;
            gap: 0.75rem;
        }

        .form-group {
            margin-bottom: 1.5rem;
        }

        .form-group label {
            font-weight: 600;
            margin-bottom: 0.5rem;
            color: var(--bs-body-color);
            display: flex;
            align-items: center;
            gap: 0.5rem;
        }

        .form-group label .required {
            color: #f5576c;
        }

        .form-control, .form-select, textarea.form-control {
            border: 2px solid rgba(102, 126, 234, 0.1);
            border-radius: var(--border-radius);
            padding: 0.75rem 1rem;
            transition: all 0.3s ease;
            background: rgba(255, 255, 255, 0.9);
        }

        [data-theme="dark"] .form-control,
        [data-theme="dark"] .form-select,
        [data-theme="dark"] textarea.form-control {
            background: rgba(255, 255, 255, 0.05);
            border-color: rgba(255, 255, 255, 0.1);
            color: var(--bs-body-color);
        }

        .form-control:focus, .form-select:focus, textarea.form-control:focus {
            border-color: var(--bs-primary);
            box-shadow: 0 0 0 0.25rem rgba(102, 126, 234, 0.25);
            transform: translateY(-2px);
        }

        textarea.form-control {
            resize: vertical;
            min-height: 100px;
        }

        /* Validation Messages */
        .text-danger {
            display: block;
            margin-top: 0.5rem;
            font-size: 0.875rem;
            animation: shake 0.5s ease;
        }

        @keyframes shake {
            0%, 100% { transform: translateX(0); }
            10%, 30%, 50%, 70%, 90% { transform: translateX(-5px); }
            20%, 40%, 60%, 80% { transform: translateX(5px); }
        }

        /* Enhanced Table */
        .products-table-container {
            background: rgba(255, 255, 255, 0.95);
            backdrop-filter: blur(10px);
            -webkit-backdrop-filter: blur(10px);
            border-radius: var(--border-radius-lg);
            overflow: hidden;
            box-shadow: var(--shadow-lg);
            border: 1px solid rgba(255, 255, 255, 0.2);
        }

        [data-theme="dark"] .products-table-container {
            background: rgba(255, 255, 255, 0.03);
            border: 1px solid rgba(255, 255, 255, 0.1);
        }

        .products-table-container .card-header {
            background: var(--primary-gradient);
            color: white;
            padding: 1.5rem;
            border: none;
        }

        .products-table-container .card-header h4 {
            margin: 0;
            font-weight: 600;
            display: flex;
            align-items: center;
            gap: 0.75rem;
        }

        .table-responsive {
            padding: 0;
        }

        .products-table {
            margin: 0;
            background: transparent;
        }

        .products-table thead th {
            background: linear-gradient(135deg, rgba(102, 126, 234, 0.1) 0%, rgba(118, 75, 162, 0.1) 100%);
            color: var(--bs-body-color);
            font-weight: 600;
            text-transform: uppercase;
            font-size: 0.875rem;
            letter-spacing: 0.05em;
            padding: 1.25rem 1rem;
            border: none;
            position: sticky;
            top: 0;
            z-index: 10;
        }

        [data-theme="dark"] .products-table thead th {
            background: linear-gradient(135deg, rgba(102, 126, 234, 0.2) 0%, rgba(118, 75, 162, 0.2) 100%);
        }

        .products-table tbody tr {
            transition: all 0.3s ease;
            border-bottom: 1px solid rgba(0, 0, 0, 0.05);
        }

        [data-theme="dark"] .products-table tbody tr {
            border-bottom: 1px solid rgba(255, 255, 255, 0.05);
        }

        .products-table tbody tr:hover {
            background: rgba(102, 126, 234, 0.05);
            transform: scale(1.01);
            box-shadow: 0 4px 12px rgba(102, 126, 234, 0.1);
        }

        [data-theme="dark"] .products-table tbody tr:hover {
            background: rgba(102, 126, 234, 0.1);
        }

        .products-table tbody td {
            padding: 1rem;
            vertical-align: middle;
        }

        /* Action Buttons in Table */
        .btn-action {
            padding: 0.375rem 0.75rem;
            font-size: 0.875rem;
            border-radius: var(--border-radius);
            transition: all 0.3s ease;
            margin: 0 0.25rem;
        }

        .btn-action:hover {
            transform: translateY(-2px);
        }

        .btn-edit {
            background: linear-gradient(135deg, #4facfe 0%, #00f2fe 100%);
            border: none;
            color: white;
        }

        .btn-delete {
            background: linear-gradient(135deg, #f5576c 0%, #f093fb 100%);
            border: none;
            color: white;
        }

        /* Empty State */
        .empty-state {
            padding: 4rem 2rem;
            text-align: center;
            color: var(--bs-gray-600);
        }

        .empty-state i {
            font-size: 4rem;
            color: var(--bs-gray-400);
            margin-bottom: 1rem;
        }

        /* Loading Overlay Enhanced */
        .loading-overlay {
            background: rgba(0, 0, 0, 0.8);
            backdrop-filter: blur(10px);
            -webkit-backdrop-filter: blur(10px);
        }

        .loading-spinner {
            width: 60px;
            height: 60px;
            border: 4px solid rgba(255, 255, 255, 0.1);
            border-top-color: var(--bs-primary);
            border-radius: 50%;
            animation: spin 1s linear infinite;
        }

        @keyframes spin {
            to { transform: rotate(360deg); }
        }

        /* Floating Action Button */
        .fab-container {
            position: fixed;
            bottom: 2rem;
            right: 2rem;
            z-index: 1000;
        }

        .fab {
            width: 60px;
            height: 60px;
            border-radius: 50%;
            background: var(--success-gradient);
            color: white;
            border: none;
            box-shadow: 0 8px 24px rgba(19, 180, 151, 0.4);
            display: flex;
            align-items: center;
            justify-content: center;
            font-size: 1.5rem;
            transition: all 0.3s cubic-bezier(0.175, 0.885, 0.32, 1.275);
            cursor: pointer;
        }

        .fab:hover {
            transform: scale(1.1) rotate(90deg);
            box-shadow: 0 12px 32px rgba(19, 180, 151, 0.5);
        }

        /* Responsive Design */
        @media (max-width: 768px) {
            .page-title {
                font-size: 1.75rem;
            }
            
            .search-section {
                padding: 1rem;
            }
            
            .btn-group-actions {
                flex-direction: column;
                gap: 0.5rem;
            }
            
            .btn-group-actions .btn {
                width: 100%;
            }
            
            .products-table {
                font-size: 0.875rem;
            }
            
            .products-table thead th {
                padding: 0.75rem 0.5rem;
                font-size: 0.75rem;
            }
            
            .products-table tbody td {
                padding: 0.75rem 0.5rem;
            }
            
            .btn-action {
                padding: 0.25rem 0.5rem;
                font-size: 0.75rem;
            }
            
            .fab-container {
                bottom: 1rem;
                right: 1rem;
            }
            
            .fab {
                width: 50px;
                height: 50px;
                font-size: 1.25rem;
            }
        }

        /* Print Styles */
        @media print {
            .navbar, .footer, .btn, .fab-container, .search-section {
                display: none !important;
            }
            
            .products-table-container {
                box-shadow: none;
                border: 1px solid #000;
            }
        }
    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <div class="container-fluid px-4">
        <!-- Page Header -->
        <div class="page-header">
            <h2 class="page-title">
                <i class="bi bi-box-seam"></i>
                Product Management
            </h2>
        </div>
        
        <!-- Alerts -->
        <asp:Panel ID="pnlError" runat="server" CssClass="alert alert-danger alert-animated" Visible="false">
            <asp:Label ID="lblError" runat="server"></asp:Label>
        </asp:Panel>
        
        <asp:Panel ID="pnlSuccess" runat="server" CssClass="alert alert-success alert-animated" Visible="false">
            <asp:Label ID="lblSuccess" runat="server"></asp:Label>
        </asp:Panel>

        <!-- Search Section -->
        <div class="search-section">
            <div class="row align-items-center">
                <div class="col-lg-8 col-md-7">
                    <div class="search-input-group">
                        <asp:TextBox ID="txtSearch" runat="server" CssClass="form-control" 
                            placeholder="Search products by name, description, or ID..."></asp:TextBox>
                    </div>
                </div>
                <div class="col-lg-4 col-md-5 mt-3 mt-md-0">
                    <div class="btn-group-actions d-flex gap-2">
                        <asp:Button ID="btnSearch" runat="server" Text="Search" 
                            CssClass="btn btn-primary flex-fill" OnClick="btnSearch_Click">
                            <i class="bi bi-search me-2"></i>
                        </asp:Button>
                        <asp:Button ID="btnShowAll" runat="server" Text="Show All" 
                            CssClass="btn btn-secondary flex-fill" OnClick="btnShowAll_Click">
                            <i class="bi bi-list-ul me-2"></i>
                        </asp:Button>
                        <asp:Button ID="btnNewProduct" runat="server" Text="New Product" 
                            CssClass="btn btn-success flex-fill" OnClick="btnNewProduct_Click">
                            <i class="bi bi-plus-circle me-2"></i>
                        </asp:Button>
                    </div>
                </div>
            </div>
        </div>

        <!-- Product Form -->
        <asp:Panel ID="pnlProductForm" runat="server" Visible="false">
            <div class="form-card">
                <div class="card-header">
                    <h4>
                        <i class="bi bi-pencil-square"></i>
                        <asp:Label ID="lblFormTitle" runat="server" Text="Product Details"></asp:Label>
                    </h4>
                </div>
                <div class="card-body p-4">
                    <asp:HiddenField ID="hdnProductId" runat="server" />
                    
                    <div class="row">
                        <div class="col-md-6">
                            <div class="form-group">
                                <label class="form-label">
                                    <i class="bi bi-tag"></i>
                                    Product Name <span class="required">*</span>
                                </label>
                                <asp:TextBox ID="txtName" runat="server" CssClass="form-control" MaxLength="100" 
                                    placeholder="Enter product name"></asp:TextBox>
                                <asp:RequiredFieldValidator ID="rfvName" runat="server" 
                                    ControlToValidate="txtName" ErrorMessage="Product name is required" 
                                    CssClass="text-danger" ValidationGroup="ProductValidation" Display="Dynamic" />
                                <asp:Label ID="lblNameError" runat="server" CssClass="text-danger" Visible="false"></asp:Label>
                            </div>
                        </div>
                        
                        <div class="col-md-6">
                            <div class="form-group">
                                <label class="form-label">
                                    <i class="bi bi-currency-dollar"></i>
                                    Price <span class="required">*</span>
                                </label>
                                <asp:TextBox ID="txtPrice" runat="server" CssClass="form-control" 
                                    TextMode="Number" step="0.01" placeholder="0.00"></asp:TextBox>
                                <asp:RequiredFieldValidator ID="rfvPrice" runat="server" 
                                    ControlToValidate="txtPrice" ErrorMessage="Price is required" 
                                    CssClass="text-danger" ValidationGroup="ProductValidation" Display="Dynamic" />
                                <asp:RangeValidator ID="rvPrice" runat="server" 
                                    ControlToValidate="txtPrice" MinimumValue="0.01" MaximumValue="999999.99" 
                                    Type="Currency" ErrorMessage="Price must be between $0.01 and $999,999.99" 
                                    CssClass="text-danger" ValidationGroup="ProductValidation" Display="Dynamic" />
                            </div>
                        </div>
                    </div>

                    <div class="form-group">
                        <label class="form-label">
                            <i class="bi bi-text-paragraph"></i>
                            Description
                        </label>
                        <asp:TextBox ID="txtDescription" runat="server" CssClass="form-control" 
                            TextMode="MultiLine" Rows="4" MaxLength="500" 
                            placeholder="Enter product description (optional)"></asp:TextBox>
                    </div>

                    <div class="row">
                        <div class="col-md-6">
                            <div class="form-group">
                                <label class="form-label">
                                    <i class="bi bi-boxes"></i>
                                    Stock Quantity <span class="required">*</span>
                                </label>
                                <asp:TextBox ID="txtStock" runat="server" CssClass="form-control" 
                                    TextMode="Number" placeholder="0"></asp:TextBox>
                                <asp:RequiredFieldValidator ID="rfvStock" runat="server" 
                                    ControlToValidate="txtStock" ErrorMessage="Stock quantity is required" 
                                    CssClass="text-danger" ValidationGroup="ProductValidation" Display="Dynamic" />
                                <asp:RangeValidator ID="rvStock" runat="server" 
                                    ControlToValidate="txtStock" MinimumValue="0" MaximumValue="999999" 
                                    Type="Integer" ErrorMessage="Stock must be between 0 and 999,999" 
                                    CssClass="text-danger" ValidationGroup="ProductValidation" Display="Dynamic" />
                            </div>
                        </div>
                    </div>

                    <div class="d-flex gap-3 mt-4">
                        <asp:Button ID="btnSave" runat="server" Text="Save Product" 
                            CssClass="btn btn-primary btn-lg" OnClick="btnSave_Click" 
                            ValidationGroup="ProductValidation">
                            <i class="bi bi-check-circle me-2"></i>
                        </asp:Button>
                        <asp:Button ID="btnCancel" runat="server" Text="Cancel" 
                            CssClass="btn btn-secondary btn-lg" OnClick="btnCancel_Click" 
                            CausesValidation="false">
                            <i class="bi bi-x-circle me-2"></i>
                        </asp:Button>
                    </div>
                </div>
            </div>
        </asp:Panel>

        <!-- Products Table -->
        <div class="products-table-container">
            <div class="card-header">
                <h4>
                    <i class="bi bi-list-ul"></i>
                    Products List
                </h4>
            </div>
            <div class="card-body p-0">
                <asp:UpdatePanel ID="upProducts" runat="server">
                    <ContentTemplate>
                        <div class="table-responsive">
                            <asp:GridView ID="gvProducts" runat="server" 
                                CssClass="table products-table table-hover mb-0" 
                                AutoGenerateColumns="False" 
                                EmptyDataText="No products found."
                                OnRowCommand="gvProducts_RowCommand"
                                DataKeyNames="Id"
                                GridLines="None">
                                <EmptyDataRowStyle CssClass="empty-state" />
                                <EmptyDataTemplate>
                                    <div class="empty-state">
                                        <i class="bi bi-inbox"></i>
                                        <h4>No Products Found</h4>
                                        <p>Start by adding your first product using the "New Product" button above.</p>
                                    </div>
                                </EmptyDataTemplate>
                                <Columns>
                                    <asp:BoundField DataField="Id" HeaderText="ID" ItemStyle-Width="80px">
                                        <ItemStyle Font-Bold="true" ForeColor="#667eea" />
                                    </asp:BoundField>
                                    <asp:BoundField DataField="Name" HeaderText="Product Name">
                                        <ItemStyle Font-Weight="500" />
                                    </asp:BoundField>
                                    <asp:BoundField DataField="Price" HeaderText="Price" 
                                        DataFormatString="{0:C}" ItemStyle-Width="120px">
                                        <ItemStyle Font-Weight="600" ForeColor="#13B497" />
                                    </asp:BoundField>
                                    <asp:BoundField DataField="StockQuantity" HeaderText="Stock" 
                                        ItemStyle-Width="100px">
                                        <ItemStyle HorizontalAlign="Center" />
                                    </asp:BoundField>
                                    <asp:BoundField DataField="CreatedDate" HeaderText="Created" 
                                        DataFormatString="{0:MMM dd, yyyy}" ItemStyle-Width="140px">
                                        <ItemStyle ForeColor="#6c757d" />
                                    </asp:BoundField>
                                    <asp:TemplateField HeaderText="Actions" ItemStyle-Width="180px">
                                        <ItemTemplate>
                                            <asp:Button ID="btnEdit" runat="server" Text="Edit" 
                                                CommandName="EditProduct" CommandArgument='<%# Eval("Id") %>'
                                                CssClass="btn btn-action btn-edit" CausesValidation="false">
                                                <i class="bi bi-pencil"></i>
                                            </asp:Button>
                                            <asp:Button ID="btnDelete" runat="server" Text="Delete" 
                                                CommandName="DeleteProduct" CommandArgument='<%# Eval("Id") %>'
                                                CssClass="btn btn-action btn-delete" CausesValidation="false"
                                                OnClientClick="return confirm('Are you sure you want to delete this product?');">
                                                <i class="bi bi-trash"></i>
                                            </asp:Button>
                                        </ItemTemplate>
                                        <ItemStyle HorizontalAlign="Center" />
                                    </asp:TemplateField>
                                </Columns>
                            </asp:GridView>
                        </div>
                    </ContentTemplate>
                    <Triggers>
                        <asp:AsyncPostBackTrigger ControlID="btnSearch" EventName="Click" />
                        <asp:AsyncPostBackTrigger ControlID="btnShowAll" EventName="Click" />
                    </Triggers>
                </asp:UpdatePanel>
            </div>
        </div>
    </div>

    <!-- Loading Indicator -->
    <asp:UpdateProgress ID="upProgress" runat="server" AssociatedUpdatePanelID="upProducts">
        <ProgressTemplate>
            <div class="loading-overlay">
                <div class="loading-spinner"></div>
            </div>
        </ProgressTemplate>
    </asp:UpdateProgress>

    <!-- Floating Action Button (Mobile) -->
    <div class="fab-container d-md-none">
        <button class="fab" onclick="document.getElementById('<%= btnNewProduct.ClientID %>').click(); return false;">
            <i class="bi bi-plus"></i>
        </button>
    </div>

    <script type="text/javascript">
        // Add smooth animations on page load
        document.addEventListener('DOMContentLoaded', function () {
            // Animate elements on scroll
            const observerOptions = {
                threshold: 0.1,
                rootMargin: '0px 0px -50px 0px'
            };

            const observer = new IntersectionObserver(function (entries) {
                entries.forEach(entry => {
                    if (entry.isIntersecting) {
                        entry.target.style.animation = 'fadeInUp 0.6s ease forwards';
                        observer.unobserve(entry.target);
                    }
                });
            }, observerOptions);

            // Observe all cards and form elements
            document.querySelectorAll('.form-card, .products-table-container').forEach(el => {
                observer.observe(el);
            });

            // Enhanced form interactions
            const formControls = document.querySelectorAll('.form-control, .form-select');
            formControls.forEach(control => {
                control.addEventListener('focus', function () {
                    this.parentElement.classList.add('focused');
                });

                control.addEventListener('blur', function () {
                    this.parentElement.classList.remove('focused');
                    if (this.value) {
                        this.parentElement.classList.add('has-value');
                    } else {
                        this.parentElement.classList.remove('has-value');
                    }
                });
            });

            // Auto-hide alerts after 5 seconds
            const alerts = document.querySelectorAll('.alert');
            alerts.forEach(alert => {
                setTimeout(() => {
                    alert.style.animation = 'fadeOut 0.5s ease forwards';
                    setTimeout(() => {
                        alert.style.display = 'none';
                    }, 500);
                }, 5000);
            });

            // Add ripple effect to buttons
            const buttons = document.querySelectorAll('.btn');
            buttons.forEach(button => {
                button.addEventListener('click', function (e) {
                    const ripple = document.createElement('span');
                    ripple.classList.add('ripple');
                    this.appendChild(ripple);

                    const rect = this.getBoundingClientRect();
                    const size = Math.max(rect.width, rect.height);
                    const x = e.clientX - rect.left - size / 2;
                    const y = e.clientY - rect.top - size / 2;

                    ripple.style.width = ripple.style.height = size + 'px';
                    ripple.style.left = x + 'px';
                    ripple.style.top = y + 'px';

                    setTimeout(() => {
                        ripple.remove();
                    }, 600);
                });
            });

            // Search field animation
            const searchInput = document.getElementById('<%= txtSearch.ClientID %>');
            if (searchInput) {
                searchInput.addEventListener('input', function() {
                    if (this.value.length > 0) {
                        this.style.borderColor = 'var(--bs-primary)';
                        this.style.boxShadow = '0 0 0 0.25rem rgba(102, 126, 234, 0.1)';
                    } else {
                        this.style.borderColor = '';
                        this.style.boxShadow = '';
                    }
                });
            }

            // Add keyboard shortcuts
            document.addEventListener('keydown', function(e) {
                // Ctrl/Cmd + N for new product
                if ((e.ctrlKey || e.metaKey) && e.key === 'n') {
                    e.preventDefault();
                    document.getElementById('<%= btnNewProduct.ClientID %>').click();
                }
                
                // Ctrl/Cmd + F for search focus
                if ((e.ctrlKey || e.metaKey) && e.key === 'f') {
                    e.preventDefault();
                    const search = document.getElementById('<%= txtSearch.ClientID %>');
                    if (search) search.focus();
                }
                
                // Escape to cancel form
                if (e.key === 'Escape') {
                    const cancelBtn = document.getElementById('<%= btnCancel.ClientID %>');
                    if (cancelBtn && cancelBtn.offsetParent !== null) {
                        cancelBtn.click();
                    }
                }
            });
        });

        // Ripple effect styles
        const style = document.createElement('style');
        style.textContent = `
            .ripple {
                position: absolute;
                border-radius: 50%;
                background: rgba(255, 255, 255, 0.6);
                transform: scale(0);
                animation: ripple-animation 0.6s ease-out;
                pointer-events: none;
            }
            
            @keyframes ripple-animation {
                to {
                    transform: scale(4);
                    opacity: 0;
                }
            }
            
            @keyframes fadeOut {
                to {
                    opacity: 0;
                    transform: translateY(-20px);
                }
            }
            
            @keyframes fadeInUp {
                from {
                    opacity: 0;
                    transform: translateY(30px);
                }
                to {
                    opacity: 1;
                    transform: translateY(0);
                }
            }
            
            .focused {
                position: relative;
            }
            
            .focused::after {
                content: '';
                position: absolute;
                bottom: 0;
                left: 0;
                right: 0;
                height: 2px;
                background: var(--primary-gradient);
                animation: expand 0.3s ease;
            }
            
            @keyframes expand {
                from {
                    transform: scaleX(0);
                }
                to {
                    transform: scaleX(1);
                }
            }
        `;
        document.head.appendChild(style);
    </script>
</asp:Content>