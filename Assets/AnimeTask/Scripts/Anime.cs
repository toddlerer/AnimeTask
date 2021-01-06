using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace AnimeTask
{
    public static class Anime
    {
        public static IScheduler DefaultScheduler { get; set; } = new TimeScheduler();

        public static UniTask Play<T>(IAnimator<T> animator, ITranslator<T> translator, CancellationToken cancellationToken = default)
        {
            return Play(animator, translator, DefaultScheduler, cancellationToken);
        }

        public static UniTask Play<T>(IAnimator<T> animator, IProgress<T> progress, CancellationToken cancellationToken = default)
        {
            return Play(animator, TranslateTo.Progress(progress), cancellationToken);
        }

        public static async UniTask Play<T>(IAnimator<T> animator, ITranslator<T> translator, IScheduler scheduler, CancellationToken cancellationToken = default)
        {
            await PlayInternal(animator, translator, scheduler, cancellationToken);
        }

        public static UniTask Play<T>(IAnimator<T> animator, IProgress<T> progress, IScheduler scheduler, CancellationToken cancellationToken = default)
        {
            return Play(animator, TranslateTo.Progress(progress), scheduler, cancellationToken);
        }

        public static UniTask PlayTo<T>(IAnimatorWithStartValue<T> animator, IValueTranslator<T> translator, CancellationToken cancellationToken = default)
        {
            return PlayTo(animator, translator, DefaultScheduler, cancellationToken);
        }

        public static async UniTask PlayTo<T>(IAnimatorWithStartValue<T> animatorWithStartValue, IValueTranslator<T> translator, IScheduler scheduler, CancellationToken cancellationToken = default)
        {
            var animator = animatorWithStartValue.Start(translator.Current);
            await PlayInternal(animator, translator, scheduler, cancellationToken);
        }

        private static async UniTask PlayInternal<T>(IAnimator<T> animator, ITranslator<T> translator, IScheduler scheduler, CancellationToken cancellationToken)
        {
            if (scheduler == default) scheduler = DefaultScheduler;

            var startTime = scheduler.Now;
            while (!cancellationToken.IsCancellationRequested && Application.isPlaying)
            {
                var time = scheduler.Now - startTime;
                var (t, used) = animator.Update(time);
                translator.Update(t);
                if (used < time) break;
                await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken);
            }
        }
    }
}
