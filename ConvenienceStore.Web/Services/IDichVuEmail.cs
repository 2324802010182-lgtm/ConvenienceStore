namespace ConvenienceStore.Web.Services
{
    public interface IDichVuEmail
    {
        Task GuiEmailAsync(string emailNhan, string tieuDe, string noiDungHtml);
    }
}