// -----------------------------------------------------------------------------
//  <auto-generated>
//      This code was generated by:
//        The WPF ShaderEffect Generator
//        http://wpfshadergenerator.codeplex.com
// 
//      Changes to this file may cause incorrect behavior and will be lost if
//      the code is regenerated.
//  </auto-generated>
// -----------------------------------------------------------------------------
namespace ShaderEffectsLibrary
{
    using System;
    using System.Windows;
    using System.Windows.Media;
    using System.Windows.Media.Effects;
    using System.Windows.Media.Media3D;
    
    
    /// <summary></summary>
    public class InvertColor : System.Windows.Media.Effects.ShaderEffect
    {
        
        public InvertColor()
        {
            PixelShader pixelShader = new PixelShader();
            pixelShader.UriSource = new Uri("/Effects;component/InvertColor.ps", UriKind.Relative);
            this.PixelShader = pixelShader;
            this.DdxUvDdyUvRegisterIndex = -1;
        }
    }
}
