using AutoMapper;
using ExpenseVista.API.DTOs.Budget;
using ExpenseVista.API.DTOs.Category;
using ExpenseVista.API.DTOs.Transaction;
using ExpenseVista.API.Models;

namespace ExpenseVista.API.Configurations
{
    public class AutoMappingConfiguration : Profile
    {
        public AutoMappingConfiguration()
        {
            MapCategory();
            MapTransaction();
            MapBudget();
        }

        public void MapCategory()
        {
            CreateMap<Category, CategoryDTO>()
                .ReverseMap();
            CreateMap<CreateCategoryDTO, Category>().ReverseMap();
            CreateMap<UpdateCategoryDTO, Category>().ReverseMap();
        }

        public void MapTransaction()
        {
            // READ: Map Category navigation property to the Category DTO
            CreateMap<Transaction, TransactionDTO>()
                .ForMember(dest => dest.Category, opt => opt.MapFrom(src => src.Category))
                .ReverseMap();

            // CREATE: Map CategoryId from DTO to the model's FK
            CreateMap<TransactionCreateDTO, Transaction>()
                .ForMember(dest => dest.CategoryId, opt => opt.MapFrom(src => src.CategoryId))
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.ApplicationUserId, opt => opt.Ignore())
                .ForMember(dest => dest.ExchangeRate, opt => opt.Ignore())
                .ForMember(dest => dest.ConvertedAmount, opt => opt.Ignore())
                .ForMember(dest => dest.TransactionDate, opt =>
                    opt.MapFrom(src => DateTime.SpecifyKind(src.TransactionDate, DateTimeKind.Utc))); ;

            // UPDATE: Map CategoryId from DTO to the model's FK
            CreateMap<TransactionUpdateDTO, Transaction>()
                .ForMember(dest => dest.CategoryId, opt => opt.MapFrom(src => src.CategoryId))
                .ForMember(dest => dest.ApplicationUserId, opt => opt.Ignore())
                .ForMember(dest => dest.ExchangeRate, opt => opt.Ignore())
                .ForMember(dest => dest.ConvertedAmount, opt => opt.Ignore());
        }
        public void MapBudget()
        {
            CreateMap<Budget, BudgetDTO>()
                .ReverseMap(); 
            CreateMap<Budget, BudgetStatusDTO>()
                .ReverseMap();

            CreateMap<BudgetCreateDTO, Budget>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.ApplicationUserId, opt => opt.Ignore());

            CreateMap<BudgetUpdateDTO, Budget>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.ApplicationUserId, opt => opt.Ignore());
        }
    }
}
