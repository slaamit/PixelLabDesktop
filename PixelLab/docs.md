# PixelLab Desktop — Code Documentation

## Overview

PixelLab is a WPF desktop application built in C# that allows users to load images, convert them between color spaces, manipulate individual color channels, visualize color spaces in 3D, and control the number of colors used in an image. It follows the **MVVM (Model–View–ViewModel)** architecture pattern.

---

## Architecture: MVVM

The project is split into three clear layers:

```
PixelLab_Desktop/
├── Models/
│   └── ImageModel.cs
├── ViewModels/
│   ├── MainViewModel.cs
│   ├── ColorSidebarViewModel.cs
│   ├── BaseViewModel.cs
│   └── RelayCommand.cs
├── Views/
│   ├── MainWindow.xaml + .cs
│   ├── ColorControlsWindow.xaml + .cs
│   └── ColorSpaceSidebar.xaml + .cs
├── Helpers/
│   ├── ColorSpaceConverter.cs
│   └── ColorSpaceVisualizer.cs
└── Services/
    └── ImageService.cs
```

**The rule of MVVM:**
- The **View** (XAML) only handles what things look like.
- The **ViewModel** holds state and logic, and notifies the View when something changes.
- The **Model** is pure data, no logic.
- The View never directly talks to the Model — it always goes through the ViewModel.

---

## File-by-File Reference

---

### `Models/ImageModel.cs`

A simple data container. Holds the file path of a loaded image and exposes the file name as a computed property.

| Property | Type | Description |
|---|---|---|
| `FilePath` | `string` | Full path to the image file on disk |
| `FileName` | `string` | Just the file name, derived from `FilePath` |

> This model is intentionally minimal and should not be changed.

---

### `ViewModels/RelayCommand.cs`

A helper class that lets you connect a C# method to a WPF button using `ICommand`. Without this, buttons in WPF can only be wired using code-behind event handlers, which breaks MVVM.

**How it works:** you pass any `Action` (a function with no parameters) to its constructor, and WPF can call it through the `ICommand` interface.

```csharp
LoadImageCommand = new RelayCommand(ExecuteLoadImage);
// Now a Button with Command="{Binding LoadImageCommand}" will call ExecuteLoadImage()
```

---

### `ViewModels/BaseViewModel.cs`

Currently empty. Intended to hold shared ViewModel infrastructure (like `OnPropertyChanged`) so that `MainViewModel` and `ColorSidebarViewModel` could both inherit from it and avoid duplicating that code. Not yet implemented.

---

### `ViewModels/MainViewModel.cs`

The brain of the application. All image processing logic lives here. It implements `INotifyPropertyChanged`, which is what allows the UI to automatically update when a property value changes.

#### State fields

| Field | Purpose |
|---|---|
| `_originalWb` | The original image as a `WriteableBitmap`. Never modified — used as the source for every operation so Reset always works. |
| `_currentImage` | The currently displayed image as a `BitmapImage`. This is what the UI shows. |
| `_imageInfo` | The status bar string shown at the bottom of the window. |
| `_originalFileName/Size/Format/Width/Height` | Cached metadata about the loaded image, used to rebuild `ImageInfo` after operations. |
| `_colorLevels` | The number of color levels for posterization (Requirement 7). Range: 2–256. |
| `_red/Green/BlueEnabled` + `Value` | Per-channel enable flag and intensity multiplier for RGB adjustment. |
| (same pattern for CMYK, HSV, YUV, YCbCr, LAB) | Same enable+value pattern repeated for each color space's channels. |

#### Commands (Requirement 2 entry points)

| Command | Calls | What it does |
|---|---|---|
| `LoadImageCommand` | `ExecuteLoadImage()` | Opens a file dialog and loads the chosen image |
| `SaveImageCommand` | `ExecuteSaveImage()` | Saves the **original** image to disk as PNG |
| `ConvertToCmykCommand` | `ExecuteConvertToCmyk()` | Converts and displays the image in CMYK space |
| `ConvertToHsvCommand` | `ExecuteConvertToHsv()` | Converts and displays in HSV |
| `ConvertToYuvCommand` | `ExecuteConvertToYuv()` | Converts and displays in YUV |
| `ConvertToYCbCrCommand` | `ExecuteConvertToYCbCr()` | Converts and displays in YCbCr |
| `ConvertToLabCommand` | `ExecuteConvertToLab()` | Converts and displays in LAB |
| `ResetToOriginalCommand` | `ExecuteResetToOriginal()` | Restores the original unmodified image |
| `ApplyPosterizeCommand` | `ApplyPosterization()` | Reduces color count to `ColorLevels` levels |

#### Key methods

**`LoadImage(path)`**
Loads an image from a file path. Creates both a `BitmapImage` (for display) and a `WriteableBitmap` (for pixel manipulation). Also extracts and stores file metadata for the status bar.

**`ApplyRgbAdjustments()` / `ApplyCmykAdjustments()` / etc.**
These are the channel control methods (Requirement 3). Each one:
1. Copies pixels from `_originalWb` (never the displayed image)
2. Converts to the relevant color space
3. Applies the enable/disable flag and the slider multiplier to each channel
4. Converts back to RGB
5. Calls `UpdateDisplay()` to show the result

**`ApplyPosterization()`**
Implements Requirement 7. Builds a 256-entry lookup table (LUT) that maps every possible byte value to its "bucket center" for the chosen number of levels, then applies it to every pixel's R, G, B channels independently.

**`UpdateDisplay(w, h, stride, pixels)`**
Takes a raw pixel array and wraps it in a `WriteableBitmap`, then converts it to a `BitmapImage` and sets `CurrentImage`. Called by every adjustment method.

**`ConvertWriteableToBitmapImage(wb)`**
WPF's `Image` control cannot bind directly to a `WriteableBitmap` and update reliably, so this method encodes it to PNG in a `MemoryStream` and loads it back as a `BitmapImage`. This is the standard WPF workaround.

> **Important:** `_originalWb` is NEVER passed to `UpdateDisplay`. It is always the read-only source. All edits work on a copy of its pixels.

---

### `ViewModels/ColorSidebarViewModel.cs`

Holds the state for the sidebar's color picker panel (Requirement 5). Completely separate from `MainViewModel` — it only cares about a single picked color and its representation in all 6 systems.

| Property | Type | Description |
|---|---|---|
| `PickedColor` | `Color` | The color picked by clicking in a 3D scene |
| `PickedBrush` | `SolidColorBrush` | Derived from `PickedColor`, used to fill the swatch |
| `RgbText` … `LabText` | `string` | Formatted value strings for each color system |
| `ActiveSpace` | `string` | Which color space tab is currently shown |

**`RefreshAllSystems()`**
Called automatically whenever `PickedColor` changes. Converts the picked RGB color into all 6 systems mathematically and updates all the text properties at once. The input is always RGB because pixel-color picking from the WebView canvas always returns an RGB value.

---

### `Services/ImageService.cs`

A small utility class with two methods:

| Method | Description |
|---|---|
| `LoadImage(path)` | Loads a file as `BitmapImage` |
| `SaveImage(bitmap, path)` | Saves a `BitmapSource` as PNG |

> Note: `MainViewModel` handles loading and saving directly rather than using this service. This class is available for future refactoring.

---

### `Helpers/ColorSpaceConverter.cs`

Pure static math functions. Each method takes a `WriteableBitmap`, reads its pixel array, applies a per-pixel color space transformation in-place, and returns a new `WriteableBitmap` with the result. The source bitmap is never modified.

| Method | Conversion |
|---|---|
| `ToCmyk(src)` | RGB → CMY approximation (maps C=255-R, M=255-G, Y=255-B, no K separation) |
| `ToHsv(src)` | RGB → HSV, encodes H/S/V as byte channels |
| `ToYuv(src)` | RGB → YUV with standard ITU coefficients |
| `ToYCbCr(src)` | RGB → YCbCr with BT.601 coefficients |
| `ToLab(src)` | RGB → sRGB linearization → XYZ (D65) → CIE L*a*b* |

> These produce a visual representation of the color space in the image display, not a lossless format conversion. The image will look different because you are looking at the raw channel values in a new space rendered as if they were RGB.

---

### `Helpers/ColorSpaceVisualizer.cs`

Generates 2D bitmap representations for color spaces. Only YUV currently uses this (as a 2D plane). All other spaces use Three.js 3D scenes in WebView2.

| Method | Output |
|---|---|
| `DrawCmykBars(w, h)` | 4 vertical gradient bars (C, M, Y, K) — legacy, no longer used in the sidebar |
| `DrawYuvPlane(w, h, yNorm)` | 2D U×V chrominance plane at a given Y (luma) level |
| `DrawYCbCrPlane(w, h, yNorm)` | 2D Cb×Cr plane at a given Y level — legacy, no longer used |
| `DrawLabPlane(w, h, lNorm)` | 2D a*×b* plane at a given L level — legacy, no longer used |

---

### `Views/MainWindow.xaml` + `MainWindow.xaml.cs`

The main application window. Layout is a two-column Grid:
- **Left column (`*`):** toolbar + image display + status bar
- **Right column (initially `0`):** the `ColorSpaceSidebar` UserControl

**Code-behind responsibilities:**
- `ToggleSidebar_Click`: shows/hides the sidebar by changing the right column width between `0` and `330`
- `OpenColorControlsWindow_Click`: creates and shows a `ColorControlsWindow`, passing the current `MainViewModel` so both windows share the same state
- `Window_Drop`: handles drag-and-drop by extracting the file path and calling `vm.LoadImageFromPath()`

**DataContext:** set to `MainViewModel` directly in XAML via `<vm:MainViewModel/>`. This means all `{Binding ...}` expressions in this window resolve against `MainViewModel`.

---

### `Views/ColorControlsWindow.xaml` + `ColorControlsWindow.xaml.cs`

A popup window with a `TabControl` that has one tab per color space (RGB, CMYK, HSV, YUV, YCbCr, LAB). Each tab contains checkboxes and sliders bound to the channel properties in `MainViewModel`.

**Important:** this window is given the same `MainViewModel` instance as `MainWindow`. So moving a slider here updates the same ViewModel that the main window is using, causing the main image to update in real time.

---

### `Views/ColorSpaceSidebar.xaml` + `ColorSpaceSidebar.xaml.cs`

A `UserControl` (reusable UI component) used as the right sidebar. Implements Requirements 4 and 5.

**Layout (stacked):**
1. Row 0 — Six buttons (RGB / HSV / CMYK / YUV / YCbCr / LAB) to switch the active 3D scene
2. Row 1 — `WebView2` control that hosts a Three.js scene for the selected color space
3. Row 2 — A thin divider line
4. Row 3 — The color info panel: a swatch + 6 rows showing the picked color's values in all systems

**3D scenes (all built as inline HTML strings in the code-behind):**

| Space | Shape | Color mapping |
|---|---|---|
| RGB | Cube | Vertex R=X, G=Y, B=Z |
| HSV | Cone (tip=black at bottom) | H=angle, S=radius, V=height |
| CMYK | Cube | Vertex C=X, M=Y, Y=Z; displayed as RGB=(1-C, 1-M, 1-Y) |
| YUV | Cylinder | Y=height, U/V=angle+radius |
| YCbCr | Cone (tip=black at bottom) | Y=height, Cb/Cr=angle+radius |
| LAB | Sphere | L=height, a*=left/right, b*=front/back |

**Controls in every 3D scene:**
- Left-drag → rotate on both X and Y axes
- Scroll wheel → zoom in/out

**Color picking:**
Uses `gl.readPixels()` on the WebGL canvas at the exact click position. This reads the actual rendered pixel color, which is always correct regardless of how much the object has been rotated. The picked RGB value is sent to C# via `window.chrome.webview.postMessage("r,g,b")`.

**`_isInitialized` flag:**
WPF fires `ValueChanged` events on sliders during `InitializeComponent()`, before all named controls exist. The `_isInitialized` flag is set to `true` only after `InitializeComponent()` finishes, and all event handlers check it before doing anything.

---

## Data Flow Summary

```
User action
    │
    ▼
View (XAML button / slider / drag-drop)
    │   WPF data binding / event handler
    ▼
ViewModel (MainViewModel or ColorSidebarViewModel)
    │   Calls helper / processes pixels
    ▼
Helper (ColorSpaceConverter / ColorSpaceVisualizer)
    │   Returns WriteableBitmap
    ▼
ViewModel sets CurrentImage / ImageInfo
    │   OnPropertyChanged notifies WPF
    ▼
View updates automatically
```

---

## Requirement → Code Map

| Requirement | Where it lives |
|---|---|
| 1 — Load image (dialog + drag & drop) | `MainViewModel.ExecuteLoadImage()`, `MainWindow.Window_Drop()` |
| 2 — Color space conversion | `MainViewModel.ExecuteConvert*()`, `ColorSpaceConverter.*` |
| 3 — Channel control (enable/disable + slider) | `MainViewModel.Apply*Adjustments()`, `ColorControlsWindow.xaml` |
| 4 — 3D color space visualization | `ColorSpaceSidebar.xaml.cs` (Three.js HTML builders) |
| 5 — Color picking + cross-system display | `ColorSpaceSidebar` picking script, `ColorSidebarViewModel.RefreshAllSystems()` |
| 6 — Real-time updates | WPF data binding + `OnPropertyChanged` throughout |
| 7 — Color count control (posterization) | `MainViewModel.ApplyPosterization()` |
| 8 — Image info display | `MainViewModel.LoadImage()`, `ImageInfo` property |
| 9 — Reset | `MainViewModel.ExecuteResetToOriginal()` |
| 10 — Save to disk | `MainViewModel.ExecuteSaveImage()` |

---

## Dependencies

| Package | Used for |
|---|---|
| `Microsoft.Web.WebView2` | Hosting Three.js 3D scenes in the sidebar |
| `Three.js r128` (CDN) | 3D rendering inside the WebView2 (loaded from cdnjs) |
| Standard WPF / .NET | Everything else |
