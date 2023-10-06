﻿//  Beat Saber Custom Avatars - Custom player models for body presence in Beat Saber.
//  Copyright © 2018-2023  Nicolas Gnyra and Beat Saber Custom Avatars Contributors
//
//  This library is free software: you can redistribute it and/or
//  modify it under the terms of the GNU Lesser General Public
//  License as published by the Free Software Foundation, either
//  version 3 of the License, or (at your option) any later version.
//
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU Lesser General Public License for more details.
//
//  You should have received a copy of the GNU Lesser General Public License
//  along with this program.  If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Reflection;

namespace CustomAvatar.Utilities
{
    internal static class ReflectionExtensions
    {
        internal static Action<TSubject, TValue> CreatePropertySetter<TSubject, TValue>(string propertyName)
        {
            PropertyInfo property = typeof(TSubject).GetProperty(propertyName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);

            if (property == null)
            {
                throw new InvalidOperationException($"Property '{propertyName}' does not exist on '{typeof(TSubject).FullName}'");
            }

            MethodInfo method = property.SetMethod;

            if (method == null)
            {
                throw new InvalidOperationException($"Property '{propertyName}' does not have a getter");
            }

            return CreateDelegate<Action<TSubject, TValue>>(method);
        }

        private static TDelegate CreateDelegate<TDelegate>(MethodInfo method) where TDelegate : Delegate
        {
            if (method == null)
            {
                throw new ArgumentNullException(nameof(method));
            }

            return (TDelegate)Delegate.CreateDelegate(typeof(TDelegate), method);
        }
    }
}
