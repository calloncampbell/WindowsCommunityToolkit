// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.Toolkit.Uwp.UI.Controls.Lottie.Animation.Keyframe;

namespace Microsoft.Toolkit.Uwp.UI.Controls.Lottie.Value
{
    /// <summary>
    /// A callback interface to be implemented by a class that will interfere with the behaviour of a given type.
    /// <seealso cref="LottieValueCallback{T}"/>
    /// </summary>
    /// <typeparam name="T">The type that the callback will work on.</typeparam>
    public interface ILottieValueCallback<T>
    {
        /// <summary>
        /// This method will be called internally to inform the callback which <see cref="IBaseKeyframeAnimation"/> it is bound to.
        /// </summary>
        /// <param name="animation">The animation that the callback should be bound to.</param>
        void SetAnimation(IBaseKeyframeAnimation animation);

        /// <summary>
        /// This method should return the appropriate value that it wants to change.
        /// Override this if you haven't set a static value in the constructor or with SetValue.
        /// </summary>
        /// <param name="frameInfo">The information of this frame, which this callback wants to change</param>
        /// <returns>Return the appropriate value that it wants to change.</returns>
        T GetValue(LottieFrameInfo<T> frameInfo);

        /// <summary>
        /// This method will be called internally from the framework from each animation and should return the appropriate value that it wants to change.
        /// </summary>
        /// <param name="startFrame">The starting frame of the current keyFrame.</param>
        /// <param name="endFrame">The ending frame of the current keyFrame.</param>
        /// <param name="startValue">The starting value of the current keyFrame.</param>
        /// <param name="endValue">The ending value of the current keyFrame.</param>
        /// <param name="linearKeyframeProgress">The linear Keyframe Progress of the current keyFrame.</param>
        /// <param name="interpolatedKeyframeProgress">The interpolated Keyframe Progress of the current keyFrame.</param>
        /// <param name="overallProgress">The overall Progress of the current keyFrame.</param>
        /// <returns>Return the appropriate value that it wants to change.</returns>
        T GetValueInternal(
            float startFrame,
            float endFrame,
            T startValue,
            T endValue,
            float linearKeyframeProgress,
            float interpolatedKeyframeProgress,
            float overallProgress);
    }
}