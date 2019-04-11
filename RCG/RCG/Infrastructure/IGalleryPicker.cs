using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace RCG.Infrastructure
{
    public interface IGalleryPicker
    {
        Task<Stream> PickFromGallery();
    }
}
