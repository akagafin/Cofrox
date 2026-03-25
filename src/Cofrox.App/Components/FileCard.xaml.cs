using System.Numerics;
using Cofrox.App.ViewModels;
using Microsoft.UI;
using Microsoft.UI.Composition;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Hosting;

namespace Cofrox.App.Components;

public sealed partial class FileCard : UserControl
{
    public FileCard()
    {
        InitializeComponent();
        Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        var visual = ElementCompositionPreview.GetElementVisual(CardBorder);
        var compositor = visual.Compositor;

        var fadeAnimation = compositor.CreateScalarKeyFrameAnimation();
        fadeAnimation.InsertKeyFrame(1, 1f);
        fadeAnimation.Duration = TimeSpan.FromMilliseconds(320);

        var offsetAnimation = compositor.CreateVector3KeyFrameAnimation();
        offsetAnimation.InsertKeyFrame(1, new Vector3(0, 0, 0));
        offsetAnimation.Duration = TimeSpan.FromMilliseconds(360);

        var collection = compositor.CreateImplicitAnimationCollection();
        collection["Offset"] = offsetAnimation;
        visual.ImplicitAnimations = collection;
        visual.Offset = new Vector3(0, 18, 0);
        visual.Opacity = 0;
        visual.StartAnimation("Opacity", fadeAnimation);
        visual.Offset = Vector3.Zero;
    }

    private void OptionPasswordChanged(object sender, RoutedEventArgs e)
    {
        if (sender is PasswordBox passwordBox && passwordBox.Tag is OptionItemViewModel option)
        {
            option.TextValue = passwordBox.Password;
        }
    }

    private void OptionColorChanged(ColorPicker sender, ColorChangedEventArgs args)
    {
        if (sender.Tag is OptionItemViewModel option)
        {
            var color = args.NewColor;
            option.TextValue = $"#{color.A:X2}{color.R:X2}{color.G:X2}{color.B:X2}";
        }
    }
}
