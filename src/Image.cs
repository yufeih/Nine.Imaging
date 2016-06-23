// ===============================================================================
// Image.cs
// .NET Image Tools
// ===============================================================================
// Copyright (c) .NET Image Tools Development Group. 
// All rights reserved.
// ===============================================================================

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Nine.Imaging.Encoding;

namespace Nine.Imaging
{
    /// <summary>
    /// Image class with stores the pixels and provides common functionality
    /// such as loading images from files and streams or operation like resizing or cutting.
    /// </summary>
    /// <remarks>The image data is alway stored in RGBA format, where the red, the blue, the
    /// alpha values are simple bytes.</remarks>
    public class Image : ImageBase
    {
        #region Constants

        private static readonly Lazy<List<IImageDecoder>> defaultDecoders = new Lazy<List<IImageDecoder>>(() => new List<IImageDecoder>
        {
            new BmpDecoder(),
            new JpegDecoder(),
            new PngDecoder(),
            new GifDecoder(),
        });

        private static readonly Lazy<List<IImageEncoder>> defaultEncoders = new Lazy<List<IImageEncoder>>(() => new List<IImageEncoder>
        {
            new BmpEncoder(),
            new JpegEncoder(),
            new PngEncoder(),
        });

        /// <summary>
        /// Gets a list of default decoders.
        /// </summary>
        public static IList<IImageDecoder> Decoders
        {
            get { return defaultDecoders.Value; }
        }

        /// <summary>
        /// Gets a list of default encoders.
        /// </summary>
        public static IList<IImageEncoder> Encoders
        {
            get { return defaultEncoders.Value; }
        }

        #endregion

        /// <summary>
        /// If not 0, this field specifies the number of hundredths (1/100) of a second to 
        /// wait before continuing with the processing of the Data Stream. 
        /// The clock starts ticking immediately after the graphic is rendered. 
        /// This field may be used in conjunction with the User Input Flag field. 
        /// </summary>
        public int? DelayTime { get; set; }

        #region Properties

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

        private IList<ImageFrame> _frames = new List<ImageFrame>();
        /// <summary>
        /// Get the other frames for the animation.
        /// </summary>
        /// <value>The list of frame images.</value>
        public IList<ImageFrame> Frames
        {
            get { return _frames; }
        }

        private IList<ImageProperty> _properties = new List<ImageProperty>();
        /// <summary>
        /// Gets the list of properties for storing meta information about this image.
        /// </summary>
        /// <value>A list of image properties.</value>
        public IList<ImageProperty> Properties
        {
            get { return _properties; }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="Image"/> class
        /// with the height and the width of the image.
        /// </summary>
        /// <param name="width">The width of the image in pixels.</param>
        /// <param name="height">The height of the image in pixels.</param>
        public Image(int width, int height) : base(width, height)
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Image"/> class
        /// by making a copy from another image.
        /// </summary>
        /// <param name="other">The other image, where the clone should be made from.</param>
        /// <exception cref="ArgumentNullException"><paramref name="other"/> is null
        /// (Nothing in Visual Basic).</exception>
        public Image(Image other) : base(other)
        {
            if (other == null) throw new ArgumentNullException("Other image cannot be null.");

            foreach (ImageFrame frame in other.Frames)
            {
                if (frame != null)
                {
                    Frames.Add(new ImageFrame(frame));
                }
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Image"/> class.
        /// </summary>
        public Image() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="Image"/> class.
        /// </summary>
        public Image(Stream stream)
        {
            if (stream == null)
            {
                throw new ArgumentNullException("stream");
            }

            Load(stream, Decoders);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Image"/> class.
        /// </summary>
        public Image(Stream stream, params IImageDecoder[] decoders)
        {
            if (stream == null)
            {
                throw new ArgumentNullException("stream");
            }

            Load(stream, decoders);
        }

        #endregion Constructors 

        #region Methods

        private void Load(Stream stream, IList<IImageDecoder> decoders)
        {
            if (!stream.CanRead)
            {
                throw new NotSupportedException("Cannot read from the stream.");
            }

            if (!stream.CanSeek)
            {
                throw new NotSupportedException("The stream does not support seeking.");
            }

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
                        return;
                    }
                }
            }

            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("Image cannot be loaded. Available decoders:");

            foreach (IImageDecoder decoder in decoders)
            {
                stringBuilder.AppendLine("-" + decoder);
            }

            throw new NotSupportedException(stringBuilder.ToString());
        }

        #endregion Methods
    }
}
