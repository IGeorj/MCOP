export const ImageInfoDisplay = ({ image, currentIndex, totalImages}: {
    image: ImageInfo;
    currentIndex: number;
    totalImages: number;
}) => (
    <div className="text-sm cursor-text select-text">
      {image?.size && <span className="ml-2 text-muted-foreground">{(image.size / 1024 / 1024).toFixed(2)} MB • ({currentIndex + 1} of {totalImages})</span>}
    </div>
);