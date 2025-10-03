using Grocery.Core.Interfaces.Repositories;
using Grocery.Core.Interfaces.Services;
using Grocery.Core.Models;

namespace Grocery.Core.Services
{
    public class GroceryListItemsService : IGroceryListItemsService
    {
        private readonly IGroceryListItemsRepository _groceriesRepository;
        private readonly IProductRepository _productRepository;

        public GroceryListItemsService(IGroceryListItemsRepository groceriesRepository, IProductRepository productRepository)
        {
            _groceriesRepository = groceriesRepository;
            _productRepository = productRepository;
        }

        public List<GroceryListItem> GetAll()
        {
            List<GroceryListItem> groceryListItems = _groceriesRepository.GetAll();
            FillService(groceryListItems);
            return groceryListItems;
        }

        public List<GroceryListItem> GetAllOnGroceryListId(int groceryListId)
        {
            List<GroceryListItem> groceryListItems = _groceriesRepository.GetAll().Where(g => g.GroceryListId == groceryListId).ToList();
            FillService(groceryListItems);
            return groceryListItems;
        }

        public GroceryListItem Add(GroceryListItem item)
        {
            return _groceriesRepository.Add(item);
        }

        public GroceryListItem? Delete(GroceryListItem item)
        {
            throw new NotImplementedException();
        }

        public GroceryListItem? Get(int id)
        {
            return _groceriesRepository.Get(id);
        }

        public GroceryListItem? Update(GroceryListItem item)
        {
            return _groceriesRepository.Update(item);
        }

        public List<BestSellingProducts> GetBestSellingProducts(int topX = 5)
        {
            List<GroceryListItem> allProducts = _groceriesRepository.GetAll();

            // group products by ProductId and calculate the total amount sold
            var ProductSales = allProducts
                .GroupBy(item => item.ProductId)  
                .Select(group => new  // create an object for each group with the Id & amount sold
                {
                    ProductId = group.Key,  
                    TotalSold = group.Sum(product => product.Amount)  
                })
                .OrderByDescending(x => x.TotalSold)  // sort highest amount sold down to the lowest
                .Take(topX)  
                .ToList();  

            List<BestSellingProducts> result = new List<BestSellingProducts>(); // make the final list to put the proper results into
            int ranking = 1; 
            foreach (var amountSold in ProductSales) // loop through the top results and make them into objects with all product info
            {
                Product? product = _productRepository.Get(amountSold.ProductId); // Get product detail from productrepository
                if (product != null)
                {
                    BestSellingProducts bestSeller = new BestSellingProducts(
                        productId: product.Id,
                        name: product.Name,
                        stock: product.Stock,
                        nrOfSells: amountSold.TotalSold,  
                        ranking: ranking  
                    );
                    result.Add(bestSeller);
                    ranking++; 
                }
            }
            return result;
        }

        private void FillService(List<GroceryListItem> groceryListItems)
        {
            foreach (GroceryListItem g in groceryListItems)
            {
                g.Product = _productRepository.Get(g.ProductId) ?? new(0, "", 0);
            }
        }
    }
}
