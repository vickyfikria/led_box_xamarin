using System;
namespace ledbox
{
    public interface IBackButton
    {
        Action BackButtonEvent { get; set; }
    }
}
