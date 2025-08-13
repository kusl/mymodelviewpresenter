<%@ Page Title="Home" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="Web.Default" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <div class="container">
        <div class="jumbotron mt-5">
            <h1 class="display-4">MVP Pattern Demo</h1>
            <p class="lead">Welcome to the Model-View-Presenter pattern demonstration using ASP.NET Web Forms and SQLite.</p>
            <hr class="my-4">
            <p>This application demonstrates:</p>
            <ul>
                <li>Clean separation of concerns with MVP pattern</li>
                <li>SQLite database for easy deployment</li>
                <li>Repository pattern for data access</li>
                <li>Async/await for database operations</li>
                <li>Bootstrap 5 for responsive UI</li>
            </ul>
            <p class="mt-4">
                <a class="btn btn-primary btn-lg" href="ProductManagement.aspx" role="button">Manage Products</a>
            </p>
        </div>
        
        <div class="row mt-5">
            <div class="col-md-4">
                <div class="card">
                    <div class="card-body">
                        <h5 class="card-title">Model</h5>
                        <p class="card-text">Business entities and repository interfaces define the data structure and contracts.</p>
                    </div>
                </div>
            </div>
            <div class="col-md-4">
                <div class="card">
                    <div class="card-body">
                        <h5 class="card-title">View</h5>
                        <p class="card-text">ASPX pages implement view interfaces and handle user interactions.</p>
                    </div>
                </div>
            </div>
            <div class="col-md-4">
                <div class="card">
                    <div class="card-body">
                        <h5 class="card-title">Presenter</h5>
                        <p class="card-text">Presenters orchestrate between models and views, containing all business logic.</p>
                    </div>
                </div>
            </div>
        </div>
    </div>
</asp:Content>