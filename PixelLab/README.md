# PixelLab Desktop

A WPF desktop application for loading, viewing, editing, and saving images with color encoding changes.

---

## Requirements

Before you clone and run this project, make sure you have the following installed:

- [Visual Studio 2022](https://visualstudio.microsoft.com/) (Community edition is free)
  - During installation, make sure to check **".NET desktop development"** workload
- [.NET 8 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)
- Git (you probably already have this)

> You do **not** need to install anything else. NuGet packages restore automatically when you build.

---

## How to Run

### 1. Clone the repository

Open Git Bash and run:

```bash
git clone <paste-the-repo-url-here>
```

Then navigate into the project folder:

```bash
cd PixelLab_Desktop
```

### 2. Open the solution

Open **Visual Studio 2022**, then:

- Click **"Open a project or solution"**
- Navigate to the cloned folder
- Select `PixelLab_Desktop.sln`

### 3. Restore NuGet packages

Visual Studio does this automatically when you open the solution. You will see it running in the bottom status bar.

If it doesn't, you can trigger it manually:
- Right-click the project in **Solution Explorer** → **Restore NuGet Packages**

### 4. Build and Run

Press **F5** or click the green **▶ Start** button.

---

## Project Structure

```
PixelLab_Desktop/
├── Models/
│   └── ImageModel.cs          # Holds image data (file path, file name)
│
├── ViewModels/
│   └── MainViewModel.cs       # All app logic (load, save, edit commands)
│
├── Views/
│   └── MainWindow.xaml        # The UI — no logic here, only layout
│   └── MainWindow.xaml.cs     # One line only: connects View to ViewModel
│
├── Services/
│   └── ImageService.cs        # Handles reading and writing image files
│
├── App.xaml                   # App entry point
└── PixelLab_Desktop.csproj    # Project configuration and dependencies
```

This project follows the **MVVM (Model-View-ViewModel)** pattern:
- **Model** — plain data, no UI
- **ViewModel** — all the logic, talks to Services
- **View** — only the UI, binds to ViewModel
- **Service** — handles file I/O and image processing

---

## Common Issues

**Build fails with NuGet errors**
→ Right-click the solution in Solution Explorer → **Restore NuGet Packages**, then try building again.

**`.NET 8 not found` error**
→ Download and install the [.NET 8 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/8.0), then restart Visual Studio.

**App opens but buttons do nothing**
→ Make sure the build succeeded with no errors (check the Output panel at the bottom).

---

## Dependencies

| Package | Version | Purpose |
|---|---|---|
| CommunityToolkit.Mvvm | 8.4.0 | MVVM base classes and source generators |

---

## Notes

- Supported image formats: PNG, JPG, JPEG, BMP
- Images are saved as PNG after editing
