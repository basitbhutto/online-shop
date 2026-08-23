(function (window, $) {
    function messageId(msg) {
        if (!msg) return '';
        return String(msg.id || msg.Id || '');
    }

    function escapeHtml(text) {
        return $('<div/>').text(text || '').html();
    }

    function formatTime(createdAt) {
        if (!createdAt) return '';
        var dt = new Date(createdAt);
        if (isNaN(dt.getTime())) return '';
        return dt.toLocaleString([], { month: 'short', day: 'numeric', hour: '2-digit', minute: '2-digit' });
    }

    function create(options) {
        options = options || {};
        var $box = $(options.container);
        var seen = {};
        var sending = false;
        var connection = null;
        var handlerBound = false;
        var currentThreadId = options.threadId || null;
        var currentUserId = options.currentUserId || '';

        function exists(id) {
            return id && (seen[id] || $box.find('[data-message-id="' + id + '"]').length > 0);
        }

        function mark(id) {
            if (id) seen[id] = true;
        }

        function render(msg) {
            var id = messageId(msg);
            if (id && exists(id)) return false;
            if (id) mark(id);
            var isAdmin = !!(msg.isFromAdmin || msg.IsFromAdmin);
            var text = msg.message || msg.Message || '';
            var createdAt = msg.createdAt || msg.CreatedAt;
            var cls = isAdmin ? 'is-admin' : 'is-buyer';
            var html = '<div class="sv-chat-msg ' + cls + '"' + (id ? ' data-message-id="' + id + '"' : '') + '>' +
                '<div class="sv-chat-msg-bubble">' + escapeHtml(text) +
                '<div class="small opacity-75 mt-1">' + formatTime(createdAt) + '</div></div></div>';
            $box.append(html);
            var el = $box[0];
            if (el) el.scrollTop = el.scrollHeight;
            return true;
        }

        function clear() {
            seen = {};
            $box.empty();
        }

        function loadMessages(threadId) {
            currentThreadId = threadId || currentThreadId;
            if (!currentThreadId) return Promise.resolve();
            $box.html('<div class="sv-empty py-4"><span class="spinner-border spinner-border-sm me-2"></span>Loading conversation...</div>');
            return fetch('/api/chat/thread/' + currentThreadId + '/messages', { credentials: 'include' })
                .then(function (r) {
                    if (r.status === 401) throw new Error('unauthorized');
                    if (!r.ok) throw new Error('load');
                    return r.json();
                })
                .then(function (msgs) {
                    clear();
                    (msgs || []).forEach(render);
                    if (!msgs || !msgs.length) {
                        $box.html('<div class="sv-empty py-4"><i class="bi bi-chat-dots"></i><h3>No messages yet</h3><p>Start the conversation below.</p></div>');
                    }
                });
        }

        function onReceive(msg) {
            var tid = currentThreadId;
            if (!tid) return;
            if (msg.threadId && String(msg.threadId) !== String(tid)) return;
            if ($box.find('.sv-empty').length) $box.empty();
            render(msg);
        }

        function ensureConnection() {
            if (typeof signalR === 'undefined') return Promise.resolve(false);
            if (connection && connection.state === signalR.HubConnectionState.Connected) return Promise.resolve(true);
            if (connection && connection.state === signalR.HubConnectionState.Connecting) {
                return connection.start().then(function () { return true; }).catch(function () { return false; });
            }
            if (connection) {
                try { connection.off('ReceiveMessage'); } catch (e) {}
            }
            connection = new signalR.HubConnectionBuilder().withUrl('/hubs/productChat').withAutomaticReconnect().build();
            connection.on('ReceiveMessage', onReceive);
            handlerBound = true;
            return connection.start().then(function () { return true; }).catch(function () { return false; });
        }

        function join(threadId) {
            currentThreadId = threadId;
            return ensureConnection().then(function (ok) {
                if (ok && connection && currentThreadId) {
                    return connection.invoke('JoinThread', currentThreadId);
                }
            });
        }

        function leave() {
            if (connection && currentThreadId) {
                connection.invoke('LeaveThread', currentThreadId).catch(function () {});
            }
        }

        function send(text, sendBtn) {
            text = (text || '').trim();
            if (!text || !currentThreadId || sending) return Promise.resolve(false);
            sending = true;
            if (window.ShopVibe) ShopVibe.setLoading(sendBtn, true, 'Sending');
            var connected = connection && typeof signalR !== 'undefined' && connection.state === signalR.HubConnectionState.Connected;

            function finish() {
                sending = false;
                if (window.ShopVibe) ShopVibe.setLoading(sendBtn, false);
            }

            if (connected) {
                return connection.invoke('SendMessage', currentThreadId, text)
                    .then(function (d) {
                        if (d) {
                            if ($box.find('.sv-empty').length) $box.empty();
                            render(d);
                        }
                    })
                    .catch(function () { return postSend(text); })
                    .finally(finish);
            }
            return postSend(text).finally(finish);
        }

        function postSend(text) {
            return fetch('/api/chat/thread/' + currentThreadId + '/send', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                credentials: 'include',
                body: JSON.stringify({ message: text })
            }).then(function (r) {
                if (r.status === 401) {
                    if (window.ShopVibe) ShopVibe.toast('Your session expired. Please sign in again.', 'error');
                    return;
                }
                if (!r.ok) throw new Error('send');
                return r.json();
            }).then(function (d) {
                if (!d) return;
                if ($box.find('.sv-empty').length) $box.empty();
                render({
                    id: d.id || d.Id,
                    message: d.message || d.Message,
                    isFromAdmin: d.isFromAdmin || d.IsFromAdmin,
                    createdAt: d.createdAt || d.CreatedAt
                });
            }).catch(function () {
                if (window.ShopVibe) ShopVibe.toast('Unable to send message. Please try again.', 'error');
            });
        }

        return {
            render: render,
            loadMessages: loadMessages,
            join: join,
            leave: leave,
            send: send,
            clear: clear,
            setThread: function (id) { currentThreadId = id; },
            getThread: function () { return currentThreadId; }
        };
    }

    window.ShopVibeChat = { create: create, messageId: messageId };
})(window, window.jQuery);
