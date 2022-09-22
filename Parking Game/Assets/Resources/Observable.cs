using System;
using System.Collections.Generic;

public class Observable {

    private object value;
    private object options;
    private readonly List<Action<object, object>> callbacks = new List<Action<object, object>>();
    
    public void Subscribe(Action<object, object> callback) {
        this.callbacks.Add(callback);
    }

    public void Subscribe(Action<object> callback) {
        this.callbacks.Add((object state, object __) => {
            callback(state);
        });
    }

    public void Subscribe(Action callback) {
        this.callbacks.Add((object _, object __) => {
            callback();
        });
    }

    public void Unsubscribe(Action callback) {
        this.callbacks.Remove((object _, object __) => {
            callback();
        });
    }

    public void Unsubscribe(Action<object> callback) {
        this.callbacks.Remove((object state, object __) => {
            callback(state);
        });
    }

    public void Unsubscribe(Action<object, object> callback) {
        this.callbacks.Remove(callback);
    }

    public void Next(object value = null, object options = null) {
        if(value != this.value || options != this.options || value == null) {
            this.value = value;
            this.options = options;
            this.TriggerCallbacks(value, options);
        }
    }

    public void Destroy() {
        this.callbacks.Clear();
    }

    private void TriggerCallbacks(object value, object options) {
        for (int i = 0; i < this.callbacks.Count; i++) {
            this.callbacks[i](value, options);
        }
    }
}
