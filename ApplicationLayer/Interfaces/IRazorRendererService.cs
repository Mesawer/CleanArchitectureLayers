using System.Threading.Tasks;
using Mesawer.ApplicationLayer.Models;

namespace Mesawer.ApplicationLayer.Interfaces
{
    public interface IRazorRendererService
    {
        Task<string> RenderAsync<TModel>(string viewName, TModel model) where TModel : EmailTemplate;
    }
}
