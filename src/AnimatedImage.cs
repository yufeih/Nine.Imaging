namespace Nine.Imaging
{
    using System.Collections.Generic;

    public class AnimatedImage : Image
    {
        public readonly int FrameDuration;

        public readonly IReadOnlyList<Image> Frames;

        public AnimatedImage(int frameDuration, IReadOnlyList<Image> frames)
            : base(frames[0].Width, frames[0].Height, frames[0].Pixels)
        {
            Frames = frames;
        }

        public override Image Clone()
        {
            var frames = new Image[Frames.Count];

            for (var i = 0; i < Frames.Count; i++)
            {
                frames[i] = Frames[i].Clone();
            }

            return new AnimatedImage(FrameDuration, frames);
        }
    }
}
