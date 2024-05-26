using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
namespace Portraiture.PlatoUI
{
    public class UIElement
    {
        public static UIElement Viewportbase = GetContainer("Viewport", 0, UIHelper.GetViewport());

        private Rectangle? _bounds;

        private SpriteEffects _flip = SpriteEffects.None;

        private Vector2 _origin = Vector2.Zero;

        private UIElement _parent;

        private float _rotation;

        public UIElement(string id = "element", Func<UIElement, UIElement, Rectangle> positioner = null, int z = 0, Texture2D theme = null, Color? color = null, float opacity = 1f, bool container = false)
        {
            Id = id;

            color ??= Color.White;

            Theme = theme;
            Color = color.Value;
            IsContainer = container;
            Opacity = opacity;

            Z = z;

            Positioner = positioner ?? UIHelper.Fill;
        }

        public UIElement Base { get; set; }
        public static UIElement DragElement { get; set; }
        public string Id { get; set; }
        public float Rotation
        {
            get
            {
                if (Parent == null)
                    return _rotation;

                return Parent.Rotation + _rotation;
            }
            set
            {
                _rotation = value;
            }
        }
        public SpriteEffects SpriteEffects
        {
            get
            {
                if (Parent == null || Parent.SpriteEffects == SpriteEffects.None || _flip != SpriteEffects.None)
                    return _flip;
                return _flip;
            }

            set
            {
                _flip = value;
            }

        }

        public Vector2 Origin
        {
            get
            {
                if (Parent == null)
                    return _origin;

                return Parent.Origin + _origin;
            }

            set
            {
                _origin = value;
            }
        }
        public List<string> Types { get; set; } = new List<string>();
        public UIElement Parent
        {
            get
            {
                if (_parent == null && this != Viewportbase)
                    _parent = Base ?? Viewportbase;

                return _parent;
            }

            set
            {
                _parent = value;
            }

        }

        public UIElement AttachedToElement { get; set; } = null;
        public List<UIElement> Children { get; set; } = new List<UIElement>();
        public int Z { get; set; }

        public bool Disabled { get; set; }

        public bool Bordered { get; set; }

        public bool Overflow { get; set; } = true;

        public bool OutOfBounds
        {
            get
            {
                return !Parent.Overflow && !Parent.OutOfBounds && !Parent.Bounds.Contains(Bounds);
            }
        }
        public bool Tiled
        {
            get
            {
                return TileSize != -1;
            }
        }

        public float TileScale
        {
            get
            {
                if (Theme != null)
                    return Theme.Width / 3f / TileSize;
                return 1f;
            }
        }
        public int TileSize { get; set; } = -1;

        public Texture2D Theme { get; set; }



        public Color Color { get; set; }

        public float Opacity { get; set; }

        public int OffsetX { get; set; }

        public int OffsetY { get; set; }

        public int AddedWidth { get; set; }

        public int AddedHeight { get; set; }

        public bool IsContainer { get; set; }

        public Action<Point, bool, UIElement> HoverAction { get; set; }

        public Action<Point, bool, bool, bool, UIElement> ClickAction { get; set; }

        public Action<GameTime, UIElement> UpdateAction { get; set; }

        public Action<Keys, bool, UIElement> KeyAction { get; set; }
        public Action<int, UIElement> ScrollAction { get; set; }

        public Action<bool, UIElement> SelectAction { get; set; }

        public Action<SpriteBatch, UIElement> DrawAction { get; set; }

        public Func<bool, Point, UIElement, bool> DragAction { get; set; }

        public Func<UIElement, UIElement, Rectangle> Positioner { get; set; }

        public Rectangle? SourceRectangle { get; set; }
        public bool WasHover { get; set; }

        public bool Visible { get; set; } = true;
        public bool IsSelected { get; set; }

        public bool IsSelectable { get; set; }

        public string SelectionId { get; set; } = "";

        public bool IsDraggable { get; set; }

        private Point? DragPosition { get; set; }
        public Vector2 DragPoint { get; set; } = Vector2.Zero;

        private Point? TempDragPoint { get; set; }
        public bool IsBeingDragged
        {
            get
            {
                return DragElement == this;
            }
        }
        public Rectangle Bounds
        {
            get
            {
                for (int i = 0; i < 100; i++)
                {
                    if (!_bounds.HasValue)
                        CalculateBounds();

                    try
                    {
                        if (_bounds.HasValue)
                        {
                            Rectangle r = new Rectangle(_bounds.Value.X + OffsetX, _bounds.Value.Y + OffsetY, _bounds.Value.Width + AddedWidth, _bounds.Value.Height + AddedHeight);

                            if (IsBeingDragged && DragPosition.HasValue)
                                return new Rectangle(DragPosition.Value.X, DragPosition.Value.Y, r.Width, r.Height);
                            return r;
                        }
                    }
                    catch
                    {
                        // ignored

                    }
                }

                return new Rectangle(0, 0, 0, 0);
            }
        }

        public UIElement Rotated(float rotation, Vector2 origin)
        {
            Rotation = rotation;
            Origin = origin;

            return this;
        }

        public UIElement Flipped(bool horizontal = true)
        {
            return this;
        }

        public UIElement WithBase(UIElement element)
        {
            Base = element;
            return this;
        }

        public UIElement WithSourceRectangle(Rectangle sourceRectangle)
        {
            SourceRectangle = sourceRectangle;
            return this;
        }

        public void Disable()
        {
            Disabled = true;

            foreach (UIElement child in Children)
                child.Disable();
        }
        public void Enable()
        {
            Disabled = false;

            foreach (UIElement child in Children)
                child.Enable();
        }

        public void CopyBasicAttributes(ref UIElement to)
        {
            to.WithInteractivity(UpdateAction, HoverAction, ClickAction, KeyAction, ScrollAction, DrawAction);

            if (IsSelectable)
                to.AsSelectable(SelectionId, SelectAction);

            if (IsDraggable)
                to.AsDraggable(DragAction, DragPoint.X, DragPoint.Y);

            if (Tiled)
                to.AsTiledBox(TileSize, Bordered);

            if (_rotation != 0 || _origin != Vector2.Zero)
                to.Rotated(_rotation, _origin);

            if (_flip != SpriteEffects.None)
                to.Flipped(_flip == SpriteEffects.FlipHorizontally);

            if (Types.Count > 0)
                to.WithTypes(Types.ToArray());
        }



        public virtual UIElement Clone(string id = null)
        {
            id ??= Id;

            UIElement e = new UIElement(id, Positioner, Z, Theme, Color, Opacity, IsContainer);
            CopyBasicAttributes(ref e);
            e.Base = null;

            foreach (UIElement child in Children)
                e.Add(child.Clone());

            return e;
        }

        public void Tranform(params object[] transformation)
        {
            int c = transformation.Count();
            for (int i = 0; i < c; i++)
            {
                if (i == 0 && transformation[i] != null)
                    OffsetX += UIHelper.GetAbs(transformation[i], Bounds.Width);

                if (i == 1 && transformation[i] != null)
                    OffsetY += UIHelper.GetAbs(transformation[i], Bounds.Height);

                if (i == 2 && transformation[i] != null)
                    AddedWidth += UIHelper.GetAbs(transformation[i], Bounds.Width);

                if (i == 3 && transformation[i] != null)
                    AddedHeight += UIHelper.GetAbs(transformation[i], Bounds.Height);
            }
        }

        public void ResetTranformation()
        {
            OffsetX = 0;
            OffsetY = 0;
            AddedWidth = 0;
            AddedHeight = 0;
        }

        public UIElement AsSelectable(string selctionId, Action<bool, UIElement> selectAction = null)
        {
            SelectAction = selectAction;
            SelectionId = selctionId;
            IsSelectable = true;
            return this;
        }

        public UIElement AsDraggable(Func<bool, Point, UIElement, bool> dragAction = null, float dragX = 0.5f, float dragY = 0.5f)
        {
            IsDraggable = true;
            DragPoint = new Vector2(dragX, dragY);
            DragAction = dragAction;
            return this;
        }

        public UIElement AsTiledBox(int tilesize, bool bordered)
        {
            TileSize = tilesize;
            Bordered = bordered;
            return this;
        }

        public void AddTypes(params string[] types)
        {
            foreach (string type in Types.Where(type => !Types.Contains(type)).ToList())
                Types.Add(type);
        }

        public void RemoveTypes(params string[] types)
        {
            foreach (string type in Types.Where(type => !Types.Contains(type)).ToList())
                Types.Remove(type);
        }

        public UIElement WithTypes(params string[] types)
        {
            Types.AddRange(types);
            return this;
        }

        public UIElement WithoutTypes()
        {
            Types.Clear();
            return this;
        }

        public void PerformUpdate(GameTime time)
        {
            if (Disabled)
                return;

            UpdateAction?.Invoke(time, this);

            foreach (UIElement child in Children)
                Task.Run(() =>
                {
                    child.PerformUpdate(time);
                });
        }

        public virtual void PerformHover(Point point)
        {
            if (Disabled || OutOfBounds)
                return;

            if (HoverAction != null && Bounds.Contains(point))
            {
                HoverAction?.Invoke(point, true, this);
                WasHover = true;
            }
            else if (HoverAction != null && WasHover)
            {
                HoverAction?.Invoke(point, false, this);
                WasHover = false;
            }

            foreach (UIElement child in Children.Where(c => c.Visible))
                Task.Run(() =>
                {
                    child.PerformHover(point);
                });
        }

        public void Deselect()
        {
            IsSelected = false;
            SelectAction?.Invoke(false, this);
        }

        public void Select()
        {
            IsSelected = true;
            SelectAction?.Invoke(true, this);
        }

        public void StopDrag(Point point)
        {
            if (IsBeingDragged)
            {
                DragAction?.Invoke(false, point, this);
                DragElement = null;
                DragPosition = null;
                TempDragPoint = null;
            }
        }

        public void PerformClick(Point point, bool right, bool release, bool hold)
        {
            if (OutOfBounds || Disabled)
                return;

            if ((ClickAction != null || IsSelectable) && Bounds.Contains(point))
            {
                if (IsSelectable && !right && release)
                {
                    if (IsSelected)
                        Deselect();
                    else
                        Select();
                }

                if (IsDraggable && !right && hold && DragElement == null && DragAction != null && DragAction.Invoke(true, point, this) && !IsBeingDragged)
                {
                    DragElement = this;
                    DragPosition = null;
                    TempDragPoint = null;
                    if (DragPoint == Vector2.Zero)
                    {
                        Rectangle b = Bounds;
                        TempDragPoint = new Point(point.X - b.X, point.Y - b.Y);
                    }
                    PerformMouseMove(point);
                }

                if (IsBeingDragged && release && DragAction != null && DragAction.Invoke(false, point, this))
                {
                    DragPosition = null;
                    DragElement = null;
                    TempDragPoint = null;
                }

                ClickAction?.Invoke(point, right, release, hold, this);
            }

            foreach (UIElement child in Children.Where(c => c.Visible))
            {
                try
                {
                    child.PerformClick(point, right, release, hold);
                }
                catch
                {
                    // ignored
                }
            }
        }

        public void PerformMouseMove(Point point)
        {
            if (Disabled)
                return;

            if (IsBeingDragged)
            {
                DragPosition = null;
                Rectangle b = Bounds;
                Point p = TempDragPoint ?? new Point(UIHelper.GetAbs(DragPoint.X, b.Width), UIHelper.GetAbs(DragPoint.Y, b.Height));
                DragPosition = new Point(point.X - p.X, point.Y - p.Y);
            }

            foreach (UIElement child in Children)
                Task.Run(() =>
                {
                    try
                    {
                        child.PerformMouseMove(point);
                    }
                    catch
                    {
                        // ignored
                    }
                });
            UpdateBounds();
        }

        public void PerformKey(Keys key, bool released)
        {
            if (Disabled)
                return;

            KeyAction?.Invoke(key, released, this);

            foreach (UIElement child in Children)
                Task.Run(() =>
                {
                    try
                    {
                        child.PerformKey(key, released);
                    }
                    catch
                    {
                        // ignored
                    }
                });
        }

        public virtual void PerformScroll(int direction)
        {
            if (OutOfBounds || Disabled)
                return;

            ScrollAction?.Invoke(direction, this);

            foreach (UIElement child in Children)
                child.PerformScroll(direction);
        }

        public void PerfromDrawAction(SpriteBatch b)
        {
            if (OutOfBounds)
                return;

            DrawAction?.Invoke(b, this);

            foreach (UIElement child in Children)
                child.PerfromDrawAction(b);
        }

        public UIElement WithInteractivity(Action<GameTime, UIElement> update = null, Action<Point, bool, UIElement> hover = null, Action<Point, bool, bool, bool, UIElement> click = null, Action<Keys, bool, UIElement> keys = null, Action<int, UIElement> scroll = null, Action<SpriteBatch, UIElement> draw = null)
        {
            if (update != null)
                UpdateAction = update;

            if (hover != null)
                HoverAction = hover;

            if (click != null)
                ClickAction = click;

            if (keys != null)
                KeyAction = keys;

            if (scroll != null)
                ScrollAction = scroll;

            if (draw != null)
                DrawAction = draw;

            return this;
        }

        public UIElement WithoutInteractivity(bool update = false, bool hover = false, bool click = false, bool keys = false, bool scroll = false, bool draw = false)
        {
            UpdateAction = update ? null : UpdateAction;
            HoverAction = hover ? null : HoverAction;
            ClickAction = click ? null : ClickAction;
            KeyAction = keys ? null : KeyAction;
            ScrollAction = scroll ? null : ScrollAction;
            DrawAction = draw ? null : DrawAction;

            return this;
        }

        public void CalculateBounds()
        {

            Positioner ??= UIHelper.Fill;
            Rectangle b = Positioner(this, Parent);
            _bounds = b;

        }

        public void UpdateBounds(bool children = true)
        {
            _bounds = null;
            CalculateBounds();

            if (children)
                foreach (UIElement child in Children)
                    Task.Run(() =>
                    {

                        child.UpdateBounds();
                    });
        }

        public IEnumerable<UIElement> GetSelected(string selectionId = null)
        {
            if (IsSelected && (selectionId == null || selectionId == SelectionId))
                yield return this;

            foreach (UIElement find in Children.SelectMany(child => child.GetSelected(selectionId)))
                yield return find;
        }

        public bool HasTypes(bool any, params string[] types)
        {
            return types.Where(_ => !any).Aggregate(true, (current, type) => Types.Contains(type) && current);
        }

        public IEnumerable<UIElement> GetElementsByType(bool any, params string[] types)
        {
            if (HasTypes(any, types))
                yield return this;

            foreach (UIElement child in Children)
                foreach (UIElement find in child.GetElementsByType(any, types))
                    yield return find;
        }

        public UIElement GetElementById(string id)
        {
            if (Id == id)
                return this;

            foreach (UIElement child in Children)
                if (child.GetElementById(id) is { } find)
                    return find;

            return null;
        }

        public static UIElement GetContainer(string id = "element", int z = 0, Func<UIElement, UIElement, Rectangle> positioner = null, float opacity = 1f)
        {
            return new UIElement(id, positioner, z, null, Color.White, opacity, true);
        }

        public static UIElement GetImage(Texture2D image, Color? color, string id = "element", float opacity = 1f, int z = 0, Func<UIElement, UIElement, Rectangle> positioner = null)
        {
            return new UIElement(id, positioner, z, image, color, opacity);
        }

        public virtual void Add(UIElement element, bool disattach = true)
        {
            if (disattach)
                element.Disattach();
            element.Parent = this;
            element.Base ??= Base;
            Children.Add(element);
        }

        public virtual void Remove(UIElement element)
        {
            Children.Remove(element);
        }

        public void Disattach()
        {
            if (Parent != null)
                Parent.Children.Remove(this);
            Parent = null;
        }

        public void Clear(UIElement element = null)
        {
            Children.Clear();
            if (element != null)
                Children.Add(element);
        }
    }
}
