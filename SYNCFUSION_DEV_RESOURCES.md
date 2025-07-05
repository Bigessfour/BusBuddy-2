# Syncfusion Development Resources (Local)

## 🔗 Quick Access Links (Available in Project Root)

### Symbolic Links Created:
- **`./Syncfusion-Samples/`** → `C:\Users\Public\Documents\Syncfusion\Windows\30.1.37`
- **`./Syncfusion-Installation/`** → `C:\Program Files (x86)\Syncfusion\Essential Studio\Windows\30.1.37`

## 📁 Key Directories for Development

### Most Useful Sample Directories:
```
./Syncfusion-Samples/
├── datagrid/                           # SfDataGrid samples
│   ├── Getting Started/
│   ├── Data Virtualization/
│   ├── Filtering/
│   ├── Grouping/
│   ├── Styling/
│   └── ...
├── calculate/                          # Calculate Engine samples
├── grid/                              # GridControl samples  
└── tools/                             # Other UI controls
```

### Installation Reference:
```
./Syncfusion-Installation/
├── Assemblies/                        # DLLs and assemblies
│   ├── 4.8/                          # .NET Framework 4.8
│   └── netcoreapp3.1/                # .NET Core 3.1
├── Help/                             # Local documentation
├── Utilities/                        # Tools and utilities
└── Windows/                         # Windows-specific resources
```

## 🎯 Quick References for Bus Buddy Development

### SfDataGrid Configuration Patterns:
1. **Initialization Pattern** (from samples):
   ```csharp
   ((ISupportInitialize)sfDataGrid).BeginInit();
   // Configure properties here
   sfDataGrid.Dock = DockStyle.Fill;
   sfDataGrid.EnableDataVirtualization = true;
   ((ISupportInitialize)sfDataGrid).EndInit();
   ```

2. **Essential Properties** (verified in samples):
   ```csharp
   sfDataGrid.AutoSizeColumnsMode = AutoSizeColumnsMode.Fill;
   sfDataGrid.AllowFiltering = true;
   sfDataGrid.AllowGrouping = true;
   sfDataGrid.ShowGroupDropArea = true;
   ```

3. **Performance Optimization** (from Data Virtualization sample):
   ```csharp
   sfDataGrid.EnableDataVirtualization = true;
   sfDataGrid.AutoSizeController.AutoSizeRange = AutoSizeRange.VisibleRows;
   ```

## 🔍 Finding Sample Code Quickly

### Command to find specific samples:
```powershell
# Find all DataGrid-related samples
Get-ChildItem "./Syncfusion-Samples" -Recurse -Filter "*DataGrid*"

# Find specific feature samples
Get-ChildItem "./Syncfusion-Samples/datagrid" | Where-Object {$_.Name -like "*Dock*" -or $_.Name -like "*Full*"}
```

### Most Relevant Samples for Bus Buddy:
- `./Syncfusion-Samples/datagrid/Getting Started/` - Basic setup patterns
- `./Syncfusion-Samples/datagrid/Data Virtualization/` - Performance optimization
- `./Syncfusion-Samples/datagrid/Styling/` - Visual theming
- `./Syncfusion-Samples/datagrid/Filtering/` - Data filtering
- `./Syncfusion-Samples/datagrid/Grouping/` - Data grouping

## 📖 Documentation Access

### Local Documentation:
- **Installation Help**: `./Syncfusion-Installation/Help/`
- **Sample Documentation**: Each sample has `sample.htm` file
- **API Reference**: `./Syncfusion-Installation/Help/API/`

### Usage in Development:
1. **Check samples first** for implementation patterns
2. **Verify with local documentation** for API details  
3. **Test with local assemblies** to ensure compatibility
4. **Reference Bus Buddy specific requirements** in project docs

## ⚡ Quick Commands for Development

```powershell
# Open samples directory
explorer ".\Syncfusion-Samples\datagrid"

# Find assembly references
Get-ChildItem ".\Syncfusion-Installation\Assemblies" -Recurse -Filter "*.dll" | Where-Object {$_.Name -like "*DataGrid*"}

# Check sample code for specific feature
Select-String -Path ".\Syncfusion-Samples\datagrid\**\*.cs" -Pattern "Dock.*Fill"
```

---
**Note**: These symbolic links make Syncfusion resources appear as if they're part of the project for easy access during development, while maintaining the mandatory local installation requirements.
