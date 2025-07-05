# Foundation & Environment Setup Checklist

This checklist ensures your development environment is ready for Bus Buddy Syncfusion WinForms development. Complete each step in order, referencing only local Syncfusion resources as required by project policy.

## 1. Visual Studio
- [ ] Install Visual Studio 2022 (Community or higher)
- [ ] Update to the latest version
- [ ] Install the ".NET desktop development" workload

## 2. .NET SDK & Runtime
- [ ] Install .NET 6.0 SDK and runtime (or version specified in Bus Buddy.csproj)
- [ ] Verify installation: `dotnet --version`

## 3. Syncfusion Essential Studio (Local Only)
- [ ] Install Syncfusion Essential Studio for Windows Forms v30.1.37
- [ ] Verify installation at: `C:\Program Files (x86)\Syncfusion\Essential Studio\Windows\30.1.37`
- [ ] Review local documentation and samples in the installation directory

## 4. Project References
- [ ] Add required Syncfusion DLLs from `...\Assemblies\4.8\` to project references (see enhancement plan for list)
- [ ] Do NOT use NuGet or online packages for Syncfusion

## 5. License Key
- [ ] Register the Syncfusion license key in `Program.cs` or startup logic
- [ ] Use the provided key from the enhancement plan

## 6. Build & Run
- [ ] Open `Bus Buddy.sln` in Visual Studio
- [ ] Build the solution (Ctrl+Shift+B)
- [ ] Run the app to verify a basic WinForms window appears

## 7. Source Control
- [ ] Initialize Git (if not already)
- [ ] Add `.gitignore` for Visual Studio and Windows
- [ ] Make initial commit and push to GitHub repository

## 8. Documentation
- [ ] Review `SYNCFUSION_PRO_LAYOUT_AND_ENHANCEMENTS.md` for project standards
- [ ] Update this checklist as you complete each step

---
**When all items are checked, mark Foundation & Environment Setup as complete in the main enhancement plan.**
