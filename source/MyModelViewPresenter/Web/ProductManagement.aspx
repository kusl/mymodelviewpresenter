<%@ Page Title="Product Management" Language="C#" MasterPageFile="~/Site.Master" 
    AutoEventWireup="true" CodeBehind="ProductManagement.aspx.cs" 
    Inherits="Web.ProductManagement" Async="true" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <div class="container">
        <h2>Product Management</h2>
        
        <asp:Panel ID="pnlError" runat="server" CssClass="alert alert-danger" Visible="false">
            <asp:Label ID="lblError" runat="server"></asp:Label>
        </asp:Panel>
        
        <asp:Panel ID="pnlSuccess" runat="server" CssClass="alert alert-success" Visible="false">
            <asp:Label ID="lblSuccess" runat="server"></asp:Label>
        </asp:Panel>

        <div class="row mb-3">
            <div class="col-md-6">
                <div class="input-group">
                    <asp:TextBox ID="txtSearch" runat="server" CssClass="form-control" 
                        placeholder="Search products..."></asp:TextBox>
                    <asp:Button ID="btnSearch" runat="server" Text="Search" 
                        CssClass="btn btn-primary" OnClick="btnSearch_Click" />
                    <asp:Button ID="btnShowAll" runat="server" Text="Show All" 
                        CssClass="btn btn-secondary" OnClick="btnShowAll_Click" />
                </div>
            </div>
            <div class="col-md-6 text-end">
                <asp:Button ID="btnNewProduct" runat="server" Text="New Product" 
                    CssClass="btn btn-success" OnClick="btnNewProduct_Click" />
            </div>
        </div>

        <asp:Panel ID="pnlProductForm" runat="server" Visible="false">
            <div class="card mb-4">
                <div class="card-header">
                    <h4><asp:Label ID="lblFormTitle" runat="server" Text="Product Details"></asp:Label></h4>
                </div>
                <div class="card-body">
                    <asp:HiddenField ID="hdnProductId" runat="server" />
                    
                    <div class="mb-3">
                        <label class="form-label">Product Name *</label>
                        <asp:TextBox ID="txtName" runat="server" CssClass="form-control" MaxLength="100"></asp:TextBox>
                        <asp:RequiredFieldValidator ID="rfvName" runat="server" 
                            ControlToValidate="txtName" ErrorMessage="Product name is required" 
                            CssClass="text-danger" ValidationGroup="ProductValidation" Display="Dynamic" />
                        <asp:Label ID="lblNameError" runat="server" CssClass="text-danger" Visible="false"></asp:Label>
                    </div>

                    <div class="mb-3">
                        <label class="form-label">Price *</label>
                        <asp:TextBox ID="txtPrice" runat="server" CssClass="form-control" 
                            TextMode="Number" step="0.01"></asp:TextBox>
                        <asp:RequiredFieldValidator ID="rfvPrice" runat="server" 
                            ControlToValidate="txtPrice" ErrorMessage="Price is required" 
                            CssClass="text-danger" ValidationGroup="ProductValidation" Display="Dynamic" />
                        <asp:RangeValidator ID="rvPrice" runat="server" 
                            ControlToValidate="txtPrice" MinimumValue="0.01" MaximumValue="999999.99" 
                            Type="Currency" ErrorMessage="Price must be between 0.01 and 999,999.99" 
                            CssClass="text-danger" ValidationGroup="ProductValidation" Display="Dynamic" />
                    </div>

                    <div class="mb-3">
                        <label class="form-label">Description</label>
                        <asp:TextBox ID="txtDescription" runat="server" CssClass="form-control" 
                            TextMode="MultiLine" Rows="3" MaxLength="500"></asp:TextBox>
                    </div>

                    <div class="mb-3">
                        <label class="form-label">Stock Quantity *</label>
                        <asp:TextBox ID="txtStock" runat="server" CssClass="form-control" 
                            TextMode="Number"></asp:TextBox>
                        <asp:RequiredFieldValidator ID="rfvStock" runat="server" 
                            ControlToValidate="txtStock" ErrorMessage="Stock quantity is required" 
                            CssClass="text-danger" ValidationGroup="ProductValidation" Display="Dynamic" />
                        <asp:RangeValidator ID="rvStock" runat="server" 
                            ControlToValidate="txtStock" MinimumValue="0" MaximumValue="999999" 
                            Type="Integer" ErrorMessage="Stock must be between 0 and 999,999" 
                            CssClass="text-danger" ValidationGroup="ProductValidation" Display="Dynamic" />
                    </div>

                    <div class="mb-3">
                        <asp:Button ID="btnSave" runat="server" Text="Save" 
                            CssClass="btn btn-primary" OnClick="btnSave_Click" 
                            ValidationGroup="ProductValidation" />
                        <asp:Button ID="btnCancel" runat="server" Text="Cancel" 
                            CssClass="btn btn-secondary ms-2" OnClick="btnCancel_Click" 
                            CausesValidation="false" />
                    </div>
                </div>
            </div>
        </asp:Panel>

        <div class="card">
            <div class="card-header">
                <h4>Products List</h4>
            </div>
            <div class="card-body">
                <asp:UpdatePanel ID="upProducts" runat="server">
                    <ContentTemplate>
                        <asp:GridView ID="gvProducts" runat="server" 
                            CssClass="table table-striped table-bordered" 
                            AutoGenerateColumns="False" 
                            EmptyDataText="No products found."
                            OnRowCommand="gvProducts_RowCommand"
                            DataKeyNames="Id">
                            <Columns>
                                <asp:BoundField DataField="Id" HeaderText="ID" ItemStyle-Width="50px" />
                                <asp:BoundField DataField="Name" HeaderText="Product Name" />
                                <asp:BoundField DataField="Price" HeaderText="Price" 
                                    DataFormatString="{0:C}" ItemStyle-Width="100px" />
                                <asp:BoundField DataField="StockQuantity" HeaderText="Stock" 
                                    ItemStyle-Width="80px" />
                                <asp:BoundField DataField="CreatedDate" HeaderText="Created" 
                                    DataFormatString="{0:MM/dd/yyyy}" ItemStyle-Width="100px" />
                                <asp:TemplateField HeaderText="Actions" ItemStyle-Width="150px">
                                    <ItemTemplate>
                                        <asp:Button ID="btnEdit" runat="server" Text="Edit" 
                                            CommandName="EditProduct" CommandArgument='<%# Eval("Id") %>'
                                            CssClass="btn btn-sm btn-primary" CausesValidation="false" />
                                        <asp:Button ID="btnDelete" runat="server" Text="Delete" 
                                            CommandName="DeleteProduct" CommandArgument='<%# Eval("Id") %>'
                                            CssClass="btn btn-sm btn-danger" CausesValidation="false"
                                            OnClientClick="return confirm('Are you sure you want to delete this product?');" />
                                    </ItemTemplate>
                                </asp:TemplateField>
                            </Columns>
                        </asp:GridView>
                    </ContentTemplate>
                    <Triggers>
                        <asp:AsyncPostBackTrigger ControlID="btnSearch" EventName="Click" />
                        <asp:AsyncPostBackTrigger ControlID="btnShowAll" EventName="Click" />
                    </Triggers>
                </asp:UpdatePanel>
            </div>
        </div>
    </div>

    <asp:UpdateProgress ID="upProgress" runat="server" AssociatedUpdatePanelID="upProducts">
        <ProgressTemplate>
            <div class="loading-overlay">
                <div class="spinner-border text-primary" role="status">
                    <span class="visually-hidden">Loading...</span>
                </div>
            </div>
        </ProgressTemplate>
    </asp:UpdateProgress>
</asp:Content>