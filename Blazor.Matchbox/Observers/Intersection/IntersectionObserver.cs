namespace Blazor.Matchbox.Observers.Intersection
{
    using System;
    using System.Collections.Generic;
    using System.Text.Json;
    using System.Threading.Tasks;

    using Microsoft.AspNetCore.Components;
    using Microsoft.JSInterop;

    public sealed class IntersectionObserver : IIntersectionObserver
    {
        private readonly IJSRuntime jsRuntime;
        private readonly Action<List<IntersectionObserverEntry>, IIntersectionObserver> callback;
        private readonly DotNetObjectReference<IntersectionObserver> reference;
        private readonly string instanceKey;
        private bool disposedValue;

        public IntersectionObserver(
            IJSRuntime jsRuntime, 
            Action<List<IntersectionObserverEntry>, IIntersectionObserver> callback, 
            IntersectionObserverOptions options)
        {
            this.jsRuntime = jsRuntime;
            this.callback = callback;
            this.instanceKey = Guid.NewGuid().ToString();
            this.reference = DotNetObjectReference.Create(this);
            this.Initialize(options);
        }

        public ValueTask DisconnectAsync()
        {
            return this.jsRuntime.InvokeVoidAsync(MethodNames.Disconnect, this.instanceKey);
        } 

        public ValueTask ObserveAsync(ElementReference targetElement)
        {
            return this.jsRuntime.InvokeVoidAsync(MethodNames.Observe, this.instanceKey, targetElement);
        }

        public ValueTask UnobserveAsync(ElementReference target)
        {
            return this.jsRuntime.InvokeVoidAsync(MethodNames.Unobserve, this.instanceKey, target);
        }

        public async ValueTask<List<IntersectionObserverEntry>> TakeRecordsAsync()
        {
            var entriesJson = await this.jsRuntime.InvokeAsync<string>(MethodNames.TakeRecords, this.instanceKey).ConfigureAwait(false);

            return DeserializeEntries(entriesJson);
        }


        [JSInvokable("InvokeCallback")]
        public void InvokeCallback(string entriesJson)
        {
            var entries = DeserializeEntries(entriesJson);
            this.callback?.Invoke(entries, this);
        }

        public async ValueTask DisposeAsync()
        {
            await this.DisposeAsync(disposing: true).ConfigureAwait(false);
        }

        private async Task DisposeAsync(bool disposing)
        {
            if (this.disposedValue)
            {
                return;
            }
            
            if (disposing)
            {
                this.reference?.Dispose();
            }

            if (this.jsRuntime != null)
            {
                await this.jsRuntime.InvokeVoidAsync(MethodNames.Dispose, this.instanceKey).ConfigureAwait(false);
            }

            this.disposedValue = true;
        }

        private static List<IntersectionObserverEntry> DeserializeEntries(string entriesJson) 
        {
            if (string.IsNullOrWhiteSpace(entriesJson))
            {
                return null;
            }
            
            var options = new JsonSerializerOptions 
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase 
            };

            return JsonSerializer.Deserialize<List<IntersectionObserverEntry>>(entriesJson, options);
        }

        private async void Initialize(IntersectionObserverOptions options) 
        {
            await this.jsRuntime.InvokeVoidAsync(MethodNames.Create, this.instanceKey, this.reference, options).ConfigureAwait(false);
        }

        private static class MethodNames
        {
            public const string Create = "BlazorMatchbox.IntersectionObserverManager.create";
            public const string TakeRecords = "BlazorMatchbox.IntersectionObserverManager.takeRecords";
            public const string Disconnect = "BlazorMatchbox.IntersectionObserverManager.disconnect";
            public const string Observe = "BlazorMatchbox.IntersectionObserverManager.observe";
            public const string Unobserve = "BlazorMatchbox.IntersectionObserverManager.unobserve";
            public const string Dispose = "BlazorMatchbox.IntersectionObserverManager.dispose";
        }
    }
}