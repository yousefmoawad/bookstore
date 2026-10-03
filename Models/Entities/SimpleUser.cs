// إذا أردت حفظه، غير اسمه لـ SimpleUser
namespace BookStore.Models.Entities
{
    public class SimpleUser
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        // ... أي خصائص أخرى
    }
}