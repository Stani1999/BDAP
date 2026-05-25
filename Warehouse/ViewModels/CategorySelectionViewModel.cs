using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Warehouse.Models;
using Warehouse.Services.Application;

namespace Warehouse.ViewModels
{
    public partial class CategorySelectionViewModel : ObservableObject
    {
        private readonly CategoryService _categoryService;

        [ObservableProperty]
        private ObservableCollection<string> _groups = new();

        [ObservableProperty]
        private ObservableCollection<Category> _categories = new();

        [ObservableProperty]
        private string _selectedGroup = string.Empty;

        [ObservableProperty]
        private Category? _selectedCategory;

        public CategorySelectionViewModel(CategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        public async Task InitializeAsync()
        {
            var groups = await _categoryService.GetAllGroupsAsync();
            Groups.Clear();
            foreach (var group in groups)
            {
                Groups.Add(group);
            }
        }

        async partial void OnSelectedGroupChanged(string value)
        {
            await LoadCategoriesForGroupAsync(value);
        }

        public async Task LoadCategoriesForGroupAsync(string group)
        {
            Categories.Clear();
            SelectedCategory = null;

            if (string.IsNullOrWhiteSpace(group)) return;

            var cats = await _categoryService.GetCategoriesByGroupAsync(group);
            foreach (var cat in cats)
            {
                Categories.Add(cat);
            }
        }
    }
}