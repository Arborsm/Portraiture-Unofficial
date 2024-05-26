﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using StardewModdingAPI;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
namespace Portraiture
{
    public static class PyShortcuts
    {
        internal static IModHelper Helper { get; } = PortraitureMod.helper;

        /* Input */


        public static object GetFieldValue(this object obj, string field, bool isStatic = false)
        {
            Type t = obj is Type ? (Type)obj : obj.GetType();
            if (obj is Type)
                isStatic = true;
            return t.GetField(field, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)?.GetValue(isStatic ? null : obj);
        }

        public static T GetFieldValue<T>(this object obj, string field, bool isStatic = false)
        {
            Type t = obj is Type ? (Type)obj : obj.GetType();
            if (obj is Type)
                isStatic = true;
            return (T)t.GetField(field, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)?.GetValue(isStatic ? null : obj);
        }

        public static void SetFieldValue(this object obj, object value, string field, bool isStatic = false)
        {
            Type t = obj is Type ? (Type)obj : obj.GetType();
            if (obj is Type)
                isStatic = true;
            t.GetField(field, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)?.SetValue(isStatic ? null : obj, value);
        }

        public static object GetPropertyValue(this object obj, string property, bool isStatic = false)
        {
            Type t = obj is Type ? (Type)obj : obj.GetType();
            if (obj is Type)
                isStatic = true;
            return t.GetProperty(property, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)?.GetValue(isStatic ? null : obj);
        }

        public static void SetPropertyValue(this object obj, object value, string property, bool isStatic = false)
        {
            if (obj is Type)
                isStatic = true;
            Type t = obj is Type ? (Type)obj : obj.GetType();
            t.GetProperty(property, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)?.SetValue(isStatic ? null : obj, value);
        }

        public static void CallAction(this object obj, string action, params object[] args)
        {
            bool isStatic = false;

            Type t = obj is Type ? (Type)obj : obj.GetType();
            if (obj is Type)
                isStatic = true;
            t.GetMethod(
                    action,
                    BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic,
                    Type.DefaultBinder,
                    args.Select(o => o.GetType()).ToArray(), Array.Empty<ParameterModifier>())
                ?.Invoke(isStatic ? null : obj, args);
        }

        public static T CallFunction<T>(this object obj, string action, params object[] args)
        {
            bool isStatic = obj is Type;

            Type t = obj is Type ? (Type)obj : obj.GetType();

            return (T)t.GetMethod(
                    action,
                    BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic,
                    Type.DefaultBinder,
                    args.Select(o => o.GetType()).ToArray(),
                    new ParameterModifier[0])
                ?.Invoke(isStatic ? null : obj, args);
        }

        public static bool isDown(this Keys k)
        {
            return Keyboard.GetState().IsKeyDown(k);
        }

        public static bool isUp(this Keys k)
        {
            return Keyboard.GetState().IsKeyUp(k);
        }

        /* Checks */

        public static bool isLocation(this string t)
        {
            return Game1.getLocationFromName(t) is not null;
        }

        /* Maps */

        public static Vector2 getTileAtMousePosition(this GameLocation t)
        {
            return new Vector2((Game1.getMouseX() + Game1.viewport.X) / (float) Game1.tileSize, (Game1.getMouseY() + Game1.viewport.Y) / (float) Game1.tileSize);
        }

        /* Converter */

        public static Vector2 toVector2(this Point p)
        {
            return new Vector2(p.X, p.Y);
        }

        public static Vector2 toVector2(this Rectangle r)
        {
            return new Vector2(r.X, r.Y);
        }

        public static Vector2 toVector2(this xTile.Dimensions.Rectangle r)
        {
            return new Vector2(r.X, r.Y);
        }

        public static Point toPoint(this Vector2 t)
        {
            return new Point((int)t.X, (int)t.Y);
        }

        public static Point toPoint(this MouseState t)
        {
            return new Point(t.X, t.Y);
        }

        public static Vector2 floorValues(this Vector2 t)
        {
            t.X = (int)t.X;
            t.Y = (int)t.Y;
            return t;
        }

        public static string toMD5Hash(this string input)
        {
            MD5 md5 = MD5.Create();
            byte[] inputBytes = Encoding.ASCII.GetBytes(input);
            byte[] hash = md5.ComputeHash(inputBytes);
            StringBuilder sb = new StringBuilder();
            foreach (byte t in hash)
            {
                sb.Append(t.ToString("X2"));
            }
            return sb.ToString();
        }

        public static T toVector<T>(this int[] arr)
        {
            if (typeof(T) == typeof(Vector2) && arr.Length > 1)
                return (T)(object)new Vector2(arr[0], arr[1]);
            if (typeof(T) == typeof(Vector3) && arr.Length > 2)
                return (T)(object)new Vector3(arr[0], arr[1], arr[2]);
            if (typeof(T) == typeof(Vector4) && arr.Length > 3)
                return (T)(object)new Vector4(arr[0], arr[1], arr[2], arr[3]);
            return (T)(object)null;
        }

        public static T toVector<T>(this List<int> arr)
        {
            return arr.ToArray().toVector<T>();
        }

        public static Color toColor(this Vector4 vec)
        {
            return new Color(vec.X, vec.Y, vec.Z, vec.W);
        }

        public static Color? toColor(this string name)
        {
            if (typeof(Color).GetProperty(name) is { } prop)
                return (Color) prop.GetValue(null)!;
            return null;
        }

        public static int toInt(this string t)
        {
            return int.Parse(t);
        }

        public static bool toBool(this string t)
        {
            return t.ToLower().Equals("true");
        }

        public static bool isNumber(this string t)
        {
            return int.TryParse(t, out int _);
        }

        public static GameLocation toLocation(this string t)
        {
            return Game1.getLocationFromName(t);
        }
    }
}
