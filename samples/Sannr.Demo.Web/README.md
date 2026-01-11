# Sannr Demo Web Application

A modern, interactive frontend for exploring Sannr's validation and sanitization features.

## 🌟 Overview

This application serves as a Gateway/Proxy to the `Sannr.Demo.ApiService`. It provides a premium user interface to:

- Test various validation scenarios in real-time.
- Observe data sanitization (e.g., trimming, case conversion).
- View enhanced error responses with performance metrics.
- Explore cross-field business rules and conditional validation.

## 🏗️ Architecture

- **Frontend**: Vanilla HTML5, CSS3 (with Glassmorphism), and modern JavaScript.
- **Backend**: ASP.NET Core 8.0 serving as a static file server and API proxy.
- **Service Discovery**: Integrates with .NET Aspire for seamless connectivity to the back-end API service.

## 🚀 Getting Started

When running via the .NET Aspire AppHost, this application will be automatically configured to proxy requests to the API service.

- `/`: Welcome page and feature overview.
- `/demo`: The interactive validation dashboard.

## 🎨 UI Features

- **Responsive Design**: Works on all devices.
- **Glassmorphism**: Modern, premium aesthetic using backdrop filters.
- **Micro-animations**: Smooth transitions and tab switching.
- **Richer Reporting**: Displays validation trace IDs and performance metrics (ms).
