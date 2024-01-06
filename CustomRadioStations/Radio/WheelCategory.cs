using System.Collections.Generic;
using System.Linq;
using GTA.Math;

namespace SelectorWheel
{
    /// <summary> A Wheel Category to hold WheelCategoryItems, <para/>
    /// Initiliazing looks for a .png image of the same category name</summary>
    public class WheelCategory
    {
        public string Name;
        public int CurrentItemIndex = 0;
        protected List<WheelCategoryItem> Items = new List<WheelCategoryItem>();
        public Vector2 position2D = Vector2.Zero;
        public Texture CategoryTexture;
        public Texture BackgroundTexture;
        public Texture HighlightTexture;

        public string Description { get; set; }

        /// <summary> Instantiates a new category for use in a selection wheel.</summary>
        /// <param name="name">Name of the category. If a matching .png image is found, the image will be displayed instead of any item image.</param>
        public WheelCategory(string name) => Name = name;

        /// <summary>Instantiates a new category for use in a selection wheel.</summary>
        /// <param name="name">Name of the category. If a matching .png image is found, the image will be displayed instead of any item image.</param>
        /// <param name="description">Category description. Only shown if there is no selected item description.</param>
        public WheelCategory(string name, string description) : this(name) => Description = description;

        /// <summary>
        /// Add item to this category.
        /// </summary>
        /// <param name="item">Item to add to this category</param>
        public void AddItem(WheelCategoryItem item) => Items.Add(item);

        public void ClearAllItems() => Items.Clear();

        public void RemoveItem(WheelCategoryItem item) => Items.Remove(item);

        public int ItemCount() => Items.Count;

        public List<WheelCategoryItem> ItemList { get { return Items; } }

        public bool IsItemSelected(WheelCategoryItem item)
        {
            if (Items.Contains(item))
            {
                return Items.IndexOf(item) == CurrentItemIndex;
            }
            return false;
        }

        public WheelCategoryItem SelectedItem
        {
            get { return Items.ElementAt(CurrentItemIndex); }
        }

        /// <summary> Sets the <see cref="CurrentItemIndex"/></summary>
        public void GoToNextItem()
        {
            if (CurrentItemIndex < Items.Count - 1)
            {
                CurrentItemIndex++;
            }
            else
            {
                CurrentItemIndex = 0;
            }
        }

        /// <summary> Sets the <see cref="CurrentItemIndex"/></summary>
        public void GoToPreviousItem()
        {
            if (CurrentItemIndex > 0)
            {
                CurrentItemIndex--;
            }
            else
            {
                CurrentItemIndex = Items.Count - 1;
            }
        }
    }
}