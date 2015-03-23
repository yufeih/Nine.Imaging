// ===============================================================================
// Image.cs
// .NET Image Tools
// ===============================================================================
// Copyright (c) .NET Image Tools Development Group. 
// All rights reserved.
// ===============================================================================

using System;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImageTools.IO;

namespace ImageTools
{
    /// <summary>
    /// Image class with stores the pixels and provides common functionality
    /// such as loading images from files and streams or operation like resizing or cutting.
    /// </summary>
    /// <remarks>The image data is alway stored in RGBA format, where the red, the blue, the
    /// alpha values are simple bytes.</remarks>
    [DebuggerDisplay("Image: {PixelWidth}x{PixelHeight}")]
    [ContractVerification(false)]
    public sealed partial class ExtendedImage : ImageBase
    {
        #region Constants

        /// <summary>
        /// The default density value (dots per inch) in x direction. The default value is 75 dots per inch.
        /// </summary>
        public const double DefaultDensityX = 75;
        /// <summary>
        /// The default density value (dots per inch) in y direction. The default value is 75 dots per inch.
        /// </summary>
        public const double DefaultDensityY = 75;

        #endregion

        #region Invariant

#if !WINDOWS_PHONE
        [ContractInvariantMethod]
        private void ImageInvariantMethod()
        {
            Contract.Invariant(_frames != null);
            Contract.Invariant(_properties != null);
        }
#endif

        #endregion

        #region Fields

        private readonly object _lockObject = new object();

        #endregion

        #region Events

        /// <summary>
        /// Occurs when the loading is completed.
        /// </summary>
        public event Action LoadingCompleted;

        /// <summary>
        /// Occurs when the loading of the image failed.
        /// </summary>
        public event Action<Exception> LoadingFailed;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets a value indicating whether this image is loading at the moment.
        /// </summary>
        /// <value>
        /// true if this instance is image is loading at the moment; otherwise, false.
        /// </value>
        public bool IsLoading { get; private set; }

        /// <summary>
        /// Gets or sets the resolution of the image in x direction. It is defined as 
        /// number of dots per inch and should be an positive value.
        /// </summary>
        /// <value>The density of the image in x direction.</value>
        public double DensityX { get; set; }

        /// <summary>
        /// Gets or sets the resolution of the image in y direction. It is defined as 
        /// number of dots per inch and should be an positive value.
        /// </summary>
        /// <value>The density of the image in y direction.</value>
        public double DensityY { get; set; }

        /// <summary>
        /// Gets the width of the image in inches. It is calculated as the width of the image 
        /// in pixels multiplied with the density. When the density is equals or less than zero 
        /// the default value is used.
        /// </summary>
        /// <value>The width of the image in inches.</value>
        public double InchWidth
        {
            get
            {
                double densityX = DensityX;

                if (densityX <= 0)
                {
                    densityX = DefaultDensityX;
                }

                return PixelWidth / densityX;
            }
        }

        /// <summary>
        /// Gets the height of the image in inches. It is calculated as the height of the image 
        /// in pixels multiplied with the density. When the density is equals or less than zero 
        /// the default value is used.
        /// </summary>
        /// <value>The height of the image in inches.</value>
        public double InchHeight
        {
            get
            {
                double densityY = DensityY;

                if (densityY <= 0)
                {
                    densityY = DefaultDensityY;
                }

                return PixelHeight / densityY;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this image is animated.
        /// </summary>
        /// <value>
        /// <c>true</c> if this image is animated; otherwise, <c>false</c>.
        /// </value>
        public bool IsAnimated
        {
            get { return _frames.Count > 0; }
        }

        private ImageFrameCollection _frames = new ImageFrameCollection();
        /// <summary>
        /// Get the other frames for the animation.
        /// </summary>
        /// <value>The list of frame images.</value>
        public ImageFrameCollection Frames
        {
            get
            {
                Contract.Ensures(Contract.Result<ImageFrameCollection>() != null);
                return _frames;
            }
        }

        private ImagePropertyCollection _properties = new ImagePropertyCollection();
        /// <summary>
        /// Gets the list of properties for storing meta information about this image.
        /// </summary>
        /// <value>A list of image properties.</value>
        public ImagePropertyCollection Properties
        {
            get
            {
                Contract.Ensures(Contract.Result<ImagePropertyCollection>() != null);
                return _properties;
            }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ExtendedImage"/> class
        /// with the height and the width of the image.
        /// </summary>
        /// <param name="width">The width of the image in pixels.</param>
        /// <param name="height">The height of the image in pixels.</param>
        public ExtendedImage(int width, int height)
            : base(width, height)
        {
            Contract.Requires<ArgumentException>(width >= 0, "Width must be greater or equals than zero.");
            Contract.Requires<ArgumentException>(height >= 0, "Height must be greater or equals than zero.");
            Contract.Ensures(IsFilled);

            DensityX = DefaultDensityX;
            DensityY = DefaultDensityY;
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="ExtendedImage"/> class
        /// by making a copy from another image.
        /// </summary>
        /// <param name="other">The other image, where the clone should be made from.</param>
        /// <exception cref="ArgumentNullException"><paramref name="other"/> is null
        /// (Nothing in Visual Basic).</exception>
        public ExtendedImage(ExtendedImage other)
            : base(other)
        {
            Contract.Requires<ArgumentNullException>(other != null, "Other image cannot be null.");
            Contract.Requires<ArgumentException>(other.IsFilled, "Other image has not been loaded.");
            Contract.Ensures(IsFilled);

            foreach (ImageFrame frame in other.Frames)
            {
                if (frame != null)
                {
                    if (!frame.IsFilled)
                    {
                        throw new ArgumentException("The image contains a frame that has not been loaded yet.");
                    }

                    Frames.Add(new ImageFrame(frame));
                }
            }

            DensityX = DefaultDensityX;
            DensityY = DefaultDensityY;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExtendedImage"/> class.
        /// </summary>
        public ExtendedImage()
        {
            DensityX = DefaultDensityX;
            DensityY = DefaultDensityY;
        }

        #endregion Constructors 

        #region Methods

        private void Load(Stream stream)
        {
            Contract.Requires(stream != null);

            try
            {
                if (!stream.CanRead)
                {
                    throw new NotSupportedException("Cannot read from the stream.");
                }

                if (!stream.CanSeek)
                {
                    throw new NotSupportedException("The stream does not support seeking.");
                }

                var decoders = Decoders.GetAvailableDecoders();

                if (decoders.Count > 0)
                {
                    int maxHeaderSize = decoders.Max(x => x.HeaderSize);
                    if (maxHeaderSize > 0)
                    {
                        byte[] header = new byte[maxHeaderSize];

                        stream.Read(header, 0, maxHeaderSize);
                        stream.Position = 0;

                        var decoder = decoders.FirstOrDefault(x => x.IsSupportedFileFormat(header));
                        if (decoder != null)
                        {
                            decoder.Decode(this, stream);
                            IsLoading = false;
                        }
                    }
                }

                if (IsLoading)
                {
                    IsLoading = false;

                    StringBuilder stringBuilder = new StringBuilder();
                    stringBuilder.AppendLine("Image cannot be loaded. Available decoders:");

                    foreach (IImageDecoder decoder in decoders)
                    {
                        stringBuilder.AppendLine("-" + decoder);
                    }

                    throw new UnsupportedImageFormatException(stringBuilder.ToString());
                }
            }
            finally
            {
                stream.Dispose();
            }
        }

        private void LoadAsync(Stream stream)
        {
            Contract.Requires(stream != null);
            Contract.Requires<InvalidOperationException>(stream.CanSeek);

            IsLoading = true;

            Task.Run(() =>
            {
                try
                {
                    Load(stream);

                    LoadingCompleted?.Invoke();
                }
                catch (Exception e)
                {
                    LoadingFailed?.Invoke(e);
                }
            });
        }

        #endregion Methods 

        #region ICloneable Members

        /// <summary>
        /// Creates a new object that is a copy of the current instance.
        /// </summary>
        /// <returns>
        /// A new object that is a copy of this instance.
        /// </returns>
        public ExtendedImage Clone()
        {
            Contract.Requires(IsFilled);
            Contract.Ensures(Contract.Result<ExtendedImage>() != null);

            return new ExtendedImage(this);
        }

        #endregion
    }
}
