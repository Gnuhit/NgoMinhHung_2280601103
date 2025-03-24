using Microsoft.EntityFrameworkCore;
using NgoMinhHung_2280601103.Models;

namespace NgoMinhHung_2280601103.Repository
{
    public class EFProductRepository : IProductRepository
    {
        private readonly ApplicationDbContext _context;

        public EFProductRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Product>> GetAllAsync()
        {
            return await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Images) // Bao gồm Images để hiển thị
                .ToListAsync();
        }

        public async Task<Product> GetByIdAsync(int id)
        {
            return await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Images) // Bao gồm Images để chỉnh sửa
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task AddAsync(Product product)
        {
            _context.Products.Add(product);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Product product)
        {
            var existingProduct = await _context.Products
                .Include(p => p.Images) // Bao gồm Images để cập nhật
                .FirstOrDefaultAsync(p => p.Id == product.Id);

            if (existingProduct != null)
            {
                // Cập nhật các thuộc tính cơ bản
                _context.Entry(existingProduct).CurrentValues.SetValues(product);

                // Xử lý Images: chỉ cập nhật nếu danh sách mới không null và không rỗng
                if (product.Images != null && product.Images.Any())
                {
                    existingProduct.Images.Clear(); // Xóa ảnh cũ
                    existingProduct.Images.AddRange(product.Images); // Thêm ảnh mới
                }
                // Nếu product.Images null hoặc rỗng, giữ nguyên ảnh cũ

                await _context.SaveChangesAsync();
            }
        }

        public async Task DeleteAsync(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product != null)
            {
                _context.Products.Remove(product);
                await _context.SaveChangesAsync();
            }
        }
    }
}