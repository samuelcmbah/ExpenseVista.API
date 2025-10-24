using AutoMapper;
using ExpenseVista.API.DTOs.ExpenseCategory;
using ExpenseVista.API.Models;

namespace ExpenseVista.API.Configurations
{
    public class AutoMappingConfiguration : Profile
    {
        public AutoMappingConfiguration()
        {
            MapExpenseCategory();
            
        }

        public void MapExpenseCategory()
        {
            CreateMap<Category, CategoryDTO>().ReverseMap();
            CreateMap<CreateCategoryDTO, Category>().ReverseMap();
        }
    }
}
