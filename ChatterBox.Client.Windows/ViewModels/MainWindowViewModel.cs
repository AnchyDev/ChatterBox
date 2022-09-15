using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatterBox.Client.Windows.ViewModels
{
    public class MainWindowViewModel : ViewModel
    {
        private string _textHelloWorld;
        public string TextHelloWorld { get => _textHelloWorld; set => SetAndNotify(ref _textHelloWorld, value); }

        public MainWindowViewModel()
        {
            TextHelloWorld = "Hello World!";
        }
    }
}
