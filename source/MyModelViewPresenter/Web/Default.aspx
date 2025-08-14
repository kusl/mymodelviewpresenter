<%@ Page Title="Home" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="Web.Default" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style>
        /* Hero Section */
        .hero-section {
            padding: 3rem 0;
            margin-bottom: 3rem;
            position: relative;
            overflow: hidden;
        }

        .hero-content {
            background: rgba(255, 255, 255, 0.95);
            backdrop-filter: blur(20px);
            -webkit-backdrop-filter: blur(20px);
            border-radius: var(--border-radius-xl);
            padding: 4rem;
            box-shadow: var(--shadow-xl);
            position: relative;
            overflow: hidden;
            border: 1px solid rgba(255, 255, 255, 0.2);
        }

        [data-theme="dark"] .hero-content {
            background: rgba(255, 255, 255, 0.03);
            border: 1px solid rgba(255, 255, 255, 0.1);
        }

        .hero-content::before {
            content: '';
            position: absolute;
            top: -50%;
            right: -50%;
            width: 200%;
            height: 200%;
            background: radial-gradient(circle, rgba(102, 126, 234, 0.1) 0%, transparent 70%);
            animation: rotate 30s linear infinite;
        }

        @keyframes rotate {
            from { transform: rotate(0deg); }
            to { transform: rotate(360deg); }
        }

        .hero-title {
            font-size: 3.5rem;
            font-weight: 800;
            background: var(--primary-gradient);
            -webkit-background-clip: text;
            -webkit-text-fill-color: transparent;
            background-clip: text;
            margin-bottom: 1.5rem;
            position: relative;
            z-index: 1;
            animation: fadeInUp 0.8s ease;
        }

        .hero-subtitle {
            font-size: 1.25rem;
            color: var(--bs-body-color);
            margin-bottom: 2rem;
            opacity: 0.9;
            position: relative;
            z-index: 1;
            animation: fadeInUp 0.8s ease 0.2s both;
        }

        /* Feature List */
        .feature-list {
            list-style: none;
            padding: 0;
            margin: 2rem 0;
            position: relative;
            z-index: 1;
        }

        .feature-list li {
            padding: 1rem 0;
            padding-left: 3rem;
            position: relative;
            font-size: 1.1rem;
            color: var(--bs-body-color);
            animation: fadeInLeft 0.8s ease both;
            transition: var(--transition-base);
        }

        .feature-list li:nth-child(1) { animation-delay: 0.3s; }
        .feature-list li:nth-child(2) { animation-delay: 0.4s; }
        .feature-list li:nth-child(3) { animation-delay: 0.5s; }
        .feature-list li:nth-child(4) { animation-delay: 0.6s; }
        .feature-list li:nth-child(5) { animation-delay: 0.7s; }

        .feature-list li:hover {
            transform: translateX(10px);
            color: var(--bs-primary);
        }

        .feature-list li::before {
            content: '';
            position: absolute;
            left: 0;
            top: 50%;
            transform: translateY(-50%);
            width: 2rem;
            height: 2rem;
            background: var(--success-gradient);
            border-radius: 50%;
            display: flex;
            align-items: center;
            justify-content: center;
            box-shadow: 0 4px 15px rgba(19, 180, 151, 0.3);
        }

        .feature-list li::after {
            content: '✓';
            position: absolute;
            left: 0.6rem;
            top: 50%;
            transform: translateY(-50%);
            color: white;
            font-weight: bold;
            font-size: 1rem;
        }

        /* Pattern Cards */
        .pattern-card {
            background: rgba(255, 255, 255, 0.95);
            backdrop-filter: blur(10px);
            -webkit-backdrop-filter: blur(10px);
            border: 1px solid rgba(255, 255, 255, 0.2);
            border-radius: var(--border-radius-lg);
            padding: 2rem;
            height: 100%;
            transition: all 0.4s cubic-bezier(0.175, 0.885, 0.32, 1.275);
            position: relative;
            overflow: hidden;
            box-shadow: var(--shadow-lg);
        }

        [data-theme="dark"] .pattern-card {
            background: rgba(255, 255, 255, 0.03);
            border: 1px solid rgba(255, 255, 255, 0.1);
        }

        .pattern-card::before {
            content: '';
            position: absolute;
            top: 0;
            left: 0;
            right: 0;
            height: 4px;
            background: var(--primary-gradient);
            transform: scaleX(0);
            transform-origin: left;
            transition: transform 0.3s ease;
        }

        .pattern-card:hover::before {
            transform: scaleX(1);
        }

        .pattern-card:hover {
            transform: translateY(-10px) scale(1.02);
            box-shadow: 0 20px 40px rgba(102, 126, 234, 0.2);
        }

        .pattern-icon {
            width: 60px;
            height: 60px;
            background: var(--primary-gradient);
            border-radius: var(--border-radius);
            display: flex;
            align-items: center;
            justify-content: center;
            margin-bottom: 1.5rem;
            font-size: 1.5rem;
            color: white;
            box-shadow: 0 8px 20px rgba(102, 126, 234, 0.3);
            transition: var(--transition-base);
        }

        .pattern-card:hover .pattern-icon {
            transform: rotate(360deg) scale(1.1);
        }

        .pattern-title {
            font-size: 1.5rem;
            font-weight: 700;
            margin-bottom: 1rem;
            background: var(--primary-gradient);
            -webkit-background-clip: text;
            -webkit-text-fill-color: transparent;
            background-clip: text;
        }

        .pattern-description {
            color: var(--bs-body-color);
            line-height: 1.6;
            opacity: 0.9;
        }

        /* CTA Button */
        .cta-button {
            display: inline-flex;
            align-items: center;
            gap: 0.75rem;
            padding: 1rem 2.5rem;
            font-size: 1.125rem;
            font-weight: 600;
            background: var(--primary-gradient);
            color: white;
            border: none;
            border-radius: var(--border-radius-lg);
            text-decoration: none;
            transition: all 0.3s cubic-bezier(0.175, 0.885, 0.32, 1.275);
            box-shadow: 0 10px 30px rgba(102, 126, 234, 0.3);
            position: relative;
            overflow: hidden;
            z-index: 1;
            animation: pulse 2s infinite;
        }

        .cta-button:hover {
            transform: translateY(-3px) scale(1.05);
            box-shadow: 0 15px 40px rgba(102, 126, 234, 0.4);
            color: white;
        }

        .cta-button::after {
            content: '';
            position: absolute;
            top: 50%;
            left: 50%;
            width: 0;
            height: 0;
            border-radius: 50%;
            background: rgba(255, 255, 255, 0.3);
            transform: translate(-50%, -50%);
            transition: width 0.6s, height 0.6s;
        }

        .cta-button:active::after {
            width: 300px;
            height: 300px;
        }

        .cta-button i {
            transition: transform 0.3s ease;
        }

        .cta-button:hover i {
            transform: translateX(5px);
        }

        /* Animations */
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

        @keyframes fadeInLeft {
            from {
                opacity: 0;
                transform: translateX(-30px);
            }
            to {
                opacity: 1;
                transform: translateX(0);
            }
        }

        /* Floating shapes */
        .floating-shape {
            position: absolute;
            opacity: 0.05;
            animation: float 20s infinite ease-in-out;
        }

        .shape-1 {
            top: 10%;
            left: 10%;
            width: 100px;
            height: 100px;
            background: var(--primary-gradient);
            border-radius: 30% 70% 70% 30% / 30% 30% 70% 70%;
            animation-delay: 0s;
        }

        .shape-2 {
            top: 60%;
            right: 10%;
            width: 150px;
            height: 150px;
            background: var(--secondary-gradient);
            border-radius: 63% 37% 54% 46% / 55% 48% 52% 45%;
            animation-delay: 5s;
        }

        .shape-3 {
            bottom: 10%;
            left: 50%;
            width: 80px;
            height: 80px;
            background: var(--success-gradient);
            border-radius: 50%;
            animation-delay: 10s;
        }

        @keyframes float {
            0%, 100% {
                transform: translateY(0) rotate(0deg);
            }
            33% {
                transform: translateY(-20px) rotate(120deg);
            }
            66% {
                transform: translateY(20px) rotate(240deg);
            }
        }

        /* Responsive adjustments */
        @media (max-width: 768px) {
            .hero-title {
                font-size: 2rem;
            }
            
            .hero-subtitle {
                font-size: 1rem;
            }
            
            .hero-content {
                padding: 2rem;
            }
            
            .pattern-card {
                margin-bottom: 1.5rem;
            }
            
            .feature-list li {
                font-size: 1rem;
            }
            
            .cta-button {
                padding: 0.875rem 2rem;
                font-size: 1rem;
            }
        }

        /* High DPI support */
        @media (-webkit-min-device-pixel-ratio: 2), (min-resolution: 192dpi) {
            .hero-title, .pattern-title {
                text-rendering: optimizeLegibility;
            }
        }
    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <div class="hero-section">
        <div class="floating-shape shape-1"></div>
        <div class="floating-shape shape-2"></div>
        <div class="floating-shape shape-3"></div>
        
        <div class="hero-content">
            <h1 class="hero-title">MVP Pattern Demo</h1>
            <p class="hero-subtitle">
                Experience the power of clean architecture with Model-View-Presenter pattern, 
                built on ASP.NET Web Forms and SQLite for ultimate simplicity and performance.
            </p>
            
            <hr class="my-4" style="background: linear-gradient(90deg, transparent, var(--bs-primary), transparent); height: 2px; border: none;">
            
            <p class="lead mb-3" style="position: relative; z-index: 1;">
                <strong>This application demonstrates:</strong>
            </p>
            
            <ul class="feature-list">
                <li>Clean separation of concerns with MVP pattern</li>
                <li>SQLite database for lightning-fast deployment</li>
                <li>Repository pattern for scalable data access</li>
                <li>Async/await for non-blocking operations</li>
                <li>Bootstrap 5 with custom theming & dark mode</li>
            </ul>
            
            <div class="mt-5" style="position: relative; z-index: 1;">
                <a class="cta-button" href="ProductManagement.aspx" role="button">
                    <span>Explore Product Management</span>
                    <i class="bi bi-arrow-right-circle"></i>
                </a>
            </div>
        </div>
    </div>
    
    <div class="row g-4 mt-5">
        <div class="col-lg-4" style="animation: fadeInUp 0.8s ease 0.8s both;">
            <div class="pattern-card">
                <div class="pattern-icon">
                    <i class="bi bi-diagram-3"></i>
                </div>
                <h3 class="pattern-title">Model</h3>
                <p class="pattern-description">
                    Business entities and repository interfaces define the data structure and contracts, 
                    ensuring type safety and clear data flow throughout the application.
                </p>
            </div>
        </div>
        
        <div class="col-lg-4" style="animation: fadeInUp 0.8s ease 0.9s both;">
            <div class="pattern-card">
                <div class="pattern-icon">
                    <i class="bi bi-window-desktop"></i>
                </div>
                <h3 class="pattern-title">View</h3>
                <p class="pattern-description">
                    ASPX pages implement view interfaces and handle user interactions with a modern, 
                    responsive UI that adapts seamlessly to any device or screen size.
                </p>
            </div>
        </div>
        
        <div class="col-lg-4" style="animation: fadeInUp 0.8s ease 1s both;">
            <div class="pattern-card">
                <div class="pattern-icon">
                    <i class="bi bi-cpu"></i>
                </div>
                <h3 class="pattern-title">Presenter</h3>
                <p class="pattern-description">
                    Presenters orchestrate between models and views, containing all business logic 
                    while maintaining testability and separation of concerns.
                </p>
            </div>
        </div>
    </div>
    
    <div class="text-center mt-5 pt-4" style="animation: fadeInUp 0.8s ease 1.2s both;">
        <p class="text-muted">
            <i class="bi bi-info-circle me-2"></i>
            Built with modern web standards for optimal performance and user experience
        </p>
    </div>
</asp:Content>