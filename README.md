# PPM Frontend Application

A modern, cross-platform mobile application built with .NET MAUI, demonstrating professional-grade mobile development practices and clean architecture principles based on MVVM Architecture.

## 🚀 Features

- **Cross-Platform Support**: Built for Android, iOS, and Windows using .NET MAUI
- **Modern UI/UX**: Custom-designed components with responsive layouts
- **Role-Based Access**: Multi-level user authentication (Admin, SuperAdmin)
- **Dashboard Analytics**: Interactive charts and statistics using LiveChartsCore
- **PDF Generation**: Document handling with iText7
- **Theme Support**: Multiple theme options with dynamic switching
- **Custom Controls**: Reusable UI components following MVVM pattern
- **Secure Authentication**: Robust login and registration system
- **Profile Management**: User profile customization and settings
- **Work Management**: Create and manage work items
- **Billing System**: Integrated billing and payment tracking

## 🛠️ Technical Stack

- **Framework**: .NET MAUI 8.0
- **Architecture**: MVVM (Model-View-ViewModel)
- **UI Components**: 
  - CommunityToolkit.Maui
  - UraniumUI.Material
  - Custom XAML controls
- **Data Visualization**: LiveChartsCore.SkiaSharpView
- **PDF Processing**: iText7
- **State Management**: CommunityToolkit.Mvvm
- **Styling**: Custom themes with dynamic resource management

## 📱 Supported Platforms

- Android (API Level 21+)
- iOS (11.0+)
- Windows 10 (10.0.17763.0+)
- MacCatalyst (13.1+)

## 🏗️ Project Structure

```
ppm-fe/
├── Controls/         # Custom UI controls
├── Models/          # Data models
├── ViewModels/      # View models for MVVM
├── Views/           # UI pages and layouts
├── Services/        # Business logic and services
├── Resources/       # Images, fonts, and themes
├── Helpers/         # Utility classes
└── Extensions/      # Extension methods
```

## 🚀 Getting Started

1. Clone the repository
2. Ensure you have the following prerequisites:
   - .NET 8.0 SDK
   - Visual Studio 2022 with MAUI workload
   - Platform-specific development tools (Android SDK, Xcode, etc.)

3. Build and run the application:
   ```bash
   dotnet build
   dotnet run
   ```

## 💡 Key Features Implementation

- **Custom Controls**: Implemented reusable UI components like `LabeledEntry`, `CustomButton`, and `EditableLabel`
- **Theme Management**: Dynamic theme switching with support for multiple color schemes
- **Data Visualization**: Interactive charts and graphs for data analysis
- **PDF Generation**: Document creation and manipulation using iText7
- **Responsive Design**: Adaptive layouts for different screen sizes and orientations

## 🔒 Security Features

- Secure authentication system
- Role-based access control
- Password encryption
- Secure data transmission

## 📦 Dependencies

- CommunityToolkit.Maui (9.0.3)
- CommunityToolkit.Mvvm (8.2.2)
- itext7 (8.0.5)
- LiveChartsCore.SkiaSharpView.Maui (2.0.0-rc4.5)
- UraniumUI.Material (2.9.1)


## 📄 License

This project is licensed under the MIT License - see the LICENSE file for details.

## 👤 Author

[Your Name]
- GitHub: [https://github.com/djzh23)]
- LinkedIn: https://www.linkedin.com/in/zouhair-ijaad/

---

Made with ❤️ using .NET MAUI 
