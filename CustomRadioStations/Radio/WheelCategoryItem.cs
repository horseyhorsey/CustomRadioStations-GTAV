namespace SelectorWheel
{
    public class WheelCategoryItem
    {
        public string Name;
        public Texture ItemTexture;
        public string Description { get; set; }

        /// <summary>
        /// Instantiate a new item to be later added to a WheelCategory.
        /// </summary>
        /// <param name="name">Name of the item. If a matching .png image is found, the image will be displayed assuming no image for this item's category has been found.</param>
        public WheelCategoryItem(string name)
        {
            Name = name;
        }

        /// <summary>
        /// Instantiate a new item to be later added to a WheelCategory.
        /// </summary>
        /// <param name="name">Name of the item. If a matching .png image is found, the image will be displayed assuming no image for this item's category has been found.</param>
        /// <param name="description">A description that will be displayed on the right side of the screen.</param>
        public WheelCategoryItem(string name, string description)
        {
            Name = name;
            Description = description;
        }
    }
}