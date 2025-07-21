# BusBuddy Control Replacement Reference

## 🎯 **Purpose**
This document catalogs all non-Syncfusion controls in the BusBuddy workspace to facilitate systematic replacement with Syncfusion equivalents for consistent theming and functionality.

---

## 📋 **Standard WPF Controls to Replace**

### 🔘 **Button Controls (41+ instances)**

**Syncfusion Replacement:** `SfButton` (in `Syncfusion.SfInput.WPF` package)

| File | Line(s) | Context | Priority |
|------|---------|---------|----------|
| `Views\Bus\NotificationWindow.xaml` | 36 | Dialog button | High |
| `Views\Bus\ConfirmationDialog.xaml` | 41, 47 | Yes/No buttons | High |
| `Views\Schedule\CancellationReasonDialog.xaml` | 33, 41 | OK/Cancel buttons | High |
| `Views\Schedule\ActivityScheduleView.xaml` | 62, 98-108 | Filter and action buttons | Medium |
| `Views\Maintenance\MaintenanceAlertsDialog.xaml` | 41, 105, 109, 114, 120 | Action buttons | Medium |
| `Views\Schedule\ActivityScheduleReportDialog.xaml` | 43 | Generate button | Medium |
| `Controls\AddressValidationControl.xaml` | ~45 | Validate button | Low |
| `Controls\QuickActionsPanel.xaml` | 68 | Quick action button | Low |

### 📝 **TextBox Controls (20+ instances)**

**Syncfusion Replacement:** `SfTextBoxExt` (in `Syncfusion.SfInput.WPF` package)

| File | Line(s) | Context | Priority |
|------|---------|---------|----------|
| `Views\StudentDetailView.xaml` | 19, 21, 23 | Student form fields | High |
| `Views\Activity\ActivityScheduleDialog.xaml` | 42, 75, 84, 93, 101 | Activity form fields | High |
| `Views\Student\AdvancedSearchDialog.xaml` | 58, 63, 139, 159, 164 | Search criteria | High |
| `Views\Student\StudentDetailView.xaml` | 39, 42, 45 | Student details | High |
| `Views\Schedule\AddEditScheduleDialog.xaml` | 116 | Schedule notes | Medium |
| `Views\Student\StudentEditView.xaml` | 67, 70, 74, 77 | Edit form fields | High |

### 📦 **ComboBox Controls (20+ instances)**

**Syncfusion Replacement:** `SfComboBox` (in `Syncfusion.SfInput.WPF` package)

| File | Line(s) | Context | Priority |
|------|---------|---------|----------|
| `Views\Fuel\FuelReconciliationDialog.xaml` | 58 | Fuel type selection | Medium |
| `Views\Student\AssignRouteDialog.xaml` | 20, 23 | Route assignment | High |
| `Views\Student\AdvancedSearchDialog.xaml` | 74, 83, 123, 131 | Search filters | High |
| `Views\Student\StudentEditView.xaml` | 84-88, 121, 129 | Gender, route selection | High |
| `Views\Student\StudentEditDialog.xaml` | 84, 88-102 | Grade selection | High |

### 📅 **DatePicker Controls (17 instances)**

**Syncfusion Replacement:** `SfDateTimePickerAdv` (in `Syncfusion.SfInput.WPF` package)

| File | Line(s) | Context | Priority |
|------|---------|---------|----------|
| `Views\Schedule\ActivityScheduleDialog.xaml` | 66 | Schedule date | High |
| `Views\Student\StudentEditDialog.xaml` | 116 | Birth date | High |
| `Views\Student\StudentEditView.xaml` | 81 | Birth date | High |
| `Views\Schedule\ActivityScheduleView.xaml` | 48, 51 | Start/End dates | High |
| `Views\Schedule\ActivityScheduleReportDialog.xaml` | 38, 41 | Report date range | Medium |
| `Views\Maintenance\MaintenanceDialog.xaml` | 59, 111, 142 | Maintenance dates | Medium |
| `Views\Fuel\FuelDialog.xaml` | 55 | Fuel date | Medium |
| `Views\Fuel\FuelReconciliationDialog.xaml` | 43, 53 | Reconciliation dates | Medium |
| `Views\Activity\ActivityTimelineView.xaml` | 63, 68 | Timeline dates | Medium |
| `Views\Bus\BusEditDialog.xaml` | 172, 187 | Inspection/Purchase dates | Low |

### ☑️ **CheckBox Controls (11 instances)**

**Syncfusion Replacement:** `SfCheckBox` (in `Syncfusion.SfInput.WPF` package)

| File | Line(s) | Context | Priority |
|------|---------|---------|----------|
| `Views\Route\RoutePlanningView.xaml` | 69, 73, 77 | Map layer toggles | Medium |
| `Views\Route\RouteManagementView.xaml` | 129 | Show inactive filter | Medium |
| `Views\Student\StudentDetailView.xaml` | 48 | Active status | High |
| `Views\Student\StudentEditDialog.xaml` | 130, 136 | Student properties | High |
| `Views\Student\StudentEditView.xaml` | 93, 96 | Active/Special needs | High |
| `Views\Maintenance\MaintenanceDialog.xaml` | 135 | Maintenance flag | Medium |
| `Views\GoogleEarth\GoogleEarthView.xaml` | 176 | View option | Low |

### 📊 **DataGrid Controls (6 instances)**

**Syncfusion Replacement:** `SfDataGrid` (in `Syncfusion.SfGrid.WPF` package)

| File | Line(s) | Context | Priority |
|------|---------|---------|----------|
| `Views\Schedule\ActivityScheduleReportDialog.xaml` | 128-136 | Statistics display | High |

---

## 🎨 **Custom Styles & Resources to Centralize**

### 🔧 **Duplicated Local Styles**

#### `ActionButton` Style
**Problem:** Defined locally in multiple files instead of centralized resource

| File | Line(s) | Definition |
|------|---------|-----------|
| `Views\Schedule\ActivityScheduleView.xaml` | 15-22 | Local style definition |
| `Views\Student\StudentEditView.xaml` | 24+ | Local style definition |

**References (using StaticResource):**
- `Views\Schedule\ActivityScheduleView.xaml`: Lines 63, 98-108
- Multiple action buttons throughout the application

#### Custom Button Styles
| File | Style Name | Line(s) | Usage |
|------|------------|---------|-------|
| `Controls\QuickActionsPanel.xaml` | `QuickActionButtonStyle` | 9-38 | Local definition and usage |
| `Controls\AddressValidationControl.xaml` | `ValidationMessageStyle` | 13-17 | TextBlock styling |
| `Controls\AddressValidationControl.xaml` | `SuccessMessageStyle` | 18-22 | TextBlock styling |

### 🔗 **Missing Resource References**

#### MainWindow Static Resources
**File:** `Views\Main\MainWindow.xaml`

| Style Name | Line(s) | Status |
|------------|---------|--------|
| `NavigationItemStyle` | 98 | ⚠️ Undefined reference |
| `MenuItemStyle` | 143, 183, 253, 305, 321 | ⚠️ Undefined reference |
| `MenuSeparatorStyle` | 168, 198, 223, 242, 278, 312 | ⚠️ Undefined reference |
| `StringEqualityConverter` | 189, 204, 210, 216, 222, 229, 235, 241, 248 | ⚠️ Undefined reference |

---

## 🔄 **Replacement Strategy**

### **Phase 1: High Priority Controls**
1. **Student Management Forms** - Replace all TextBox, ComboBox, DatePicker, CheckBox in student-related views
2. **Dialog Buttons** - Replace all Button controls in dialogs with SfButton
3. **Data Entry Forms** - Replace form controls in Activity and Schedule management

### **Phase 2: Medium Priority Controls**
1. **Maintenance & Fuel Forms** - Replace controls in maintenance and fuel management
2. **Report Dialogs** - Replace DataGrid with SfDataGrid
3. **Filter Controls** - Replace search and filter controls

### **Phase 3: Low Priority Controls**
1. **Custom Controls** - Update controls in `/Controls/` folder
2. **Utility Views** - Replace controls in less critical views

### **Phase 4: Style Centralization**
1. **Move ActionButton style** to `Resources/Themes/FluentDark.xaml`
2. **Create missing style definitions** for MainWindow references
3. **Consolidate validation styles** into theme resources
4. **Add StringEqualityConverter** to application resources

---

## 📋 **Implementation Checklist**

### **Per-File Replacement Tasks**

#### Example Template for `Views\Student\StudentEditView.xaml`:
- [ ] Replace `<TextBox>` with `<syncfusion:SfTextBoxExt>` (Lines 67, 70, 74, 77)
- [ ] Replace `<DatePicker>` with `<syncfusion:SfDateTimePickerAdv>` (Line 81)
- [ ] Replace `<ComboBox>` with `<syncfusion:SfComboBox>` (Lines 84-88, 121, 129)
- [ ] Replace `<CheckBox>` with `<syncfusion:SfCheckBox>` (Lines 93, 96)
- [ ] Move `ActionButton` style to central theme resource
- [ ] Update style references to use centralized resources

### **Resource Dictionary Updates**
- [ ] Create `Resources/Themes/ControlStyles.xaml` for centralized button styles
- [ ] Add missing converter definitions to `App.xaml` resources
- [ ] Update all `StaticResource` references to point to centralized resources
- [ ] Validate all namespace declarations include Syncfusion references

### **Testing Requirements**
- [ ] Verify visual consistency across all replaced controls
- [ ] Test data binding functionality after control replacement
- [ ] Validate theme switching (FluentDark/FluentLight) works correctly
- [ ] Check accessibility and keyboard navigation
- [ ] Performance testing for large data sets (SfDataGrid)

---

## 🎯 **Expected Benefits**

1. **Visual Consistency** - Uniform look and feel across all controls
2. **Theme Integration** - Proper FluentDark/FluentLight theme support
3. **Enhanced Functionality** - Advanced features from Syncfusion controls
4. **Maintainability** - Centralized styling and theming
5. **Performance** - Optimized Syncfusion control performance
6. **Accessibility** - Better accessibility support in Syncfusion controls

---

## ⚠️ **Migration Notes**

### **Breaking Changes to Consider**
- Property name differences between WPF and Syncfusion controls
- Event handler signatures may differ
- Data binding syntax adjustments may be needed
- Custom styling properties will need updates

### **Namespace Updates Required**
Add to all affected XAML files:
```xml
xmlns:syncfusion="http://schemas.syncfusion.com/wpf"
xmlns:syncfusionskin="clr-namespace:Syncfusion.SfSkinManager;assembly=Syncfusion.SfSkinManager.WPF"
```

### **Package Dependencies**
Ensure all files using replaced controls reference:
- `Syncfusion.SfInput.WPF` (for SfTextBoxExt, SfComboBox, SfCheckBox, SfButton)
- `Syncfusion.SfGrid.WPF` (for SfDataGrid)
- `Syncfusion.SfSkinManager.WPF` (for theming)

---

*Generated: July 20, 2025*
*Status: Ready for implementation*
