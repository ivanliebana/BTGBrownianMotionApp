using System.Threading.Tasks;

#if WINDOWS
using Microsoft.Maui.Controls;
#endif

namespace BTG.BrownianMotionApp.Services.Interfaces;

public interface IWindowService
{
    // Closes the application with confirmation (Windows-only)
    Task CloseApplicationAsync(Page? anchor = null, bool askConfirm = true);
}
