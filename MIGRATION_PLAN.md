# MS Controls → Syncfusion Migration Plan

## Phase 1: Core Input Controls (Priority 1)

### 1. Button → ButtonAdv
```xml
<!-- OLD MS Control -->
<Button Content="Save" Click="SaveButton_Click" Style="{StaticResource PrimaryButtonStyle}" />

<!-- NEW Syncfusion Control -->
<syncfusion:ButtonAdv Content="Save" Click="SaveButton_Click" Style="{StaticResource PrimaryButtonAdvStyle}" />
```

### 2. TextBox → SfTextBoxExt
```xml
<!-- OLD MS Control -->
<TextBox Text="{Binding Name}" Style="{StaticResource StandardTextBoxStyle}" />

<!-- NEW Syncfusion Control -->
<syncfusion:SfTextBoxExt Text="{Binding Name}" Style="{StaticResource BusBuddySfTextBoxExtStyle}" />
```

### 3. ComboBox → ComboBoxAdv
```xml
<!-- OLD MS Control -->
<ComboBox ItemsSource="{Binding Items}" SelectedItem="{Binding SelectedItem}" />

<!-- NEW Syncfusion Control -->
<syncfusion:ComboBoxAdv ItemsSource="{Binding Items}" SelectedItem="{Binding SelectedItem}"
                        Style="{StaticResource BusBuddyComboBoxAdvStyle}" />
```

## Phase 2: Data Display Controls (Priority 2)

### 1. DataGrid → SfDataGrid
```xml
<!-- OLD MS Control -->
<DataGrid ItemsSource="{Binding Data}" AutoGenerateColumns="False">
    <DataGrid.Columns>
        <DataGridTextColumn Header="Name" Binding="{Binding Name}" />
    </DataGrid.Columns>
</DataGrid>

<!-- NEW Syncfusion Control -->
<syncfusion:SfDataGrid ItemsSource="{Binding Data}" AutoGenerateColumns="False"
                       Style="{StaticResource BusBuddySfDataGridStyle}">
    <syncfusion:SfDataGrid.Columns>
        <syncfusion:GridTextColumn HeaderText="Name" MappingName="Name" />
    </syncfusion:SfDataGrid.Columns>
</syncfusion:SfDataGrid>
```

### 2. ListView → SfTreeView
```xml
<!-- OLD MS Control -->
<ListView ItemsSource="{Binding Items}" Style="{StaticResource BusBuddyListViewStyle}" />

<!-- NEW Syncfusion Control -->
<syncfusion:SfTreeView ItemsSource="{Binding Items}" Style="{StaticResource BusBuddySfTreeViewStyle}" />
```

## Phase 3: Layout & Navigation (Priority 3)

### 1. TabControl → TabControlExt
```xml
<!-- OLD MS Control -->
<TabControl>
    <TabItem Header="Tab 1">
        <TextBlock Text="Content 1" />
    </TabItem>
</TabControl>

<!-- NEW Syncfusion Control -->
<syncfusion:TabControlExt>
    <syncfusion:TabItemExt Header="Tab 1">
        <TextBlock Text="Content 1" />
    </syncfusion:TabItemExt>
</syncfusion:TabControlExt>
```

## Benefits of Migration

### 1. **Consistent Theme Integration**
- All controls automatically use FluentDark/FluentLight themes
- No more MS Controls resource dictionary issues
- Perfect theme consistency across the application

### 2. **Enhanced Features**
- `SfDataGrid` has built-in filtering, sorting, grouping
- `ButtonAdv` has enhanced styling and animation
- `ComboBoxAdv` has better performance and features

### 3. **Better Performance**
- Syncfusion controls are optimized for WPF
- Better memory management
- Improved virtualization

### 4. **Pure Syncfusion Environment**
- No mixed control theming issues
- Single vendor support
- Consistent API patterns

## Migration Commands

### Required Namespace Updates
```xml
<!-- Add to XAML files -->
xmlns:syncfusion="http://schemas.syncfusion.com/wpf"
```

### Style Updates Required
All existing styles need to be updated to target Syncfusion controls:
- `PrimaryButtonStyle` → `PrimaryButtonAdvStyle`
- `StandardTextBoxStyle` → `BusBuddySfTextBoxExtStyle`
- `BusBuddyListViewStyle` → `BusBuddySfTreeViewStyle`

## Implementation Steps

1. **Update one control type at a time**
2. **Test thoroughly after each change**
3. **Update corresponding styles in BusBuddyResourceDictionary.xaml**
4. **Remove MS Controls styles once migration is complete**
5. **Remove MS Controls resource dictionary references**

## Files to Update

### XAML Files (Views)
- All View files in `BusBuddy.WPF/Views/`
- `MainWindow.xaml`
- Any UserControl files

### Resource Files
- `BusBuddyResourceDictionary.xaml` - Update styles
- `App.xaml` - Remove MS Controls references

### Code-Behind Files
- Update event handlers if control APIs differ
- Update any direct control references in ViewModels

## Timeline Estimate
- **Phase 1**: 2-3 hours (Core controls)
- **Phase 2**: 3-4 hours (Data controls)
- **Phase 3**: 2-3 hours (Layout controls)
- **Testing**: 2-3 hours
- **Total**: 8-12 hours for complete migration

## Expected Outcome
- **100% Pure Syncfusion Environment**
- **No MS Controls resource dictionary issues**
- **Consistent FluentDark/FluentLight theming**
- **Enhanced features and performance**
- **Simplified maintenance**
