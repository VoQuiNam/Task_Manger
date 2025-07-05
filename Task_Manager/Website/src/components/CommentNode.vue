<template>
    <div class="d-flex mb-3 align-items-start" :style="{ marginLeft: `${level * 20}px` }">
        <img src="https://via.placeholder.com/40" class="rounded-circle me-2" width="40" height="40" />

        <div class="d-flex flex-column flex-grow-1">
            <div class="d-flex align-items-baseline mb-1">
                <strong class="me-2">{{ comment.FullName || 'Unknown' }}</strong>
                <small class="text-muted">{{ timeAgoFormatted }}</small>
            </div>

            <div v-if="editingCommentId === comment.CommentID">
                <textarea class="form-control" :value="editedContent[comment.CommentID] || ''"
                    @input="$emit('update:editedContent', comment.CommentID, $event.target.value)"
                    @keydown.enter.prevent="$emit('submit-edit', comment.CommentID, editedContent[comment.CommentID])">

</textarea>

                <div class="mt-1">
                    <button class="btn btn-sm btn-secondary" @click="$emit('cancel-edit')">Cancel</button>
                </div>
            </div>
            <div v-else class="mb-1 text-start" style="white-space: pre-wrap;">
                {{ comment.Content }}
            </div>

            <div class="text-muted small d-flex align-items-center gap-2">
                <span class="cursor-pointer" @click="$emit('reply', comment)">Reply</span>
                <span class="cursor-pointer" @click="$emit('edit', comment)">Edit</span>
                <span class="cursor-pointer" @click="$emit('delete', comment.CommentID)">Delete</span>
            </div>

            <!-- Reply form -->
            <div v-if="replyingToCommentId === comment.CommentID" class="mt-2">
                <textarea class="form-control" :value="replyContents[comment.CommentID] || ''"
                    @input="$emit('update:replyContents', comment.CommentID, $event.target.value)"
                    @keydown.enter.prevent="$emit('submit-reply', comment.CommentID)"></textarea>




                <div class="mt-1">
                    <button class="btn btn-sm btn-primary"
                        @click="$emit('submit-reply', comment.CommentID)">Reply</button>
                    <button class="btn btn-sm btn-secondary" @click="$emit('cancel-reply')">Cancel</button>
                </div>
            </div>

            <!-- Children (recursive) -->
            <CommentNode v-for="child in comment.children" :key="child.CommentID" :comment="child" :level="level + 1"
                :editingCommentId="editingCommentId" :replyingToCommentId="replyingToCommentId"
                :replyContents="replyContents" :editedContent="editedContent" @reply="$emit('reply', $event)"
                @edit="$emit('edit', $event)" @delete="$emit('delete', $event)"
                @submit-reply="$emit('submit-reply', $event)" @cancel-reply="$emit('cancel-reply')"
                @submit-edit="(id, content) => $emit('submit-edit', id, content)" @cancel-edit="$emit('cancel-edit')"
                @update:replyContents="(id, content) => $emit('update:replyContents', id, content)"
                @update:editedContent="(id, content) => $emit('update:editedContent', id, content)" />

        </div>
    </div>
</template>

<script>
import { timeAgo } from '@/assets/js/commentnode.js';

export default {
    name: 'CommentNode',
    props: {
        comment: Object,
        level: { type: Number, default: 0 },
        editingCommentId: Number,
        replyingToCommentId: Number,
        replyContents: { type: Object, required: true },
        editedContent: Object
    },
    data() {
        return {
            now: new Date(), // ✅ biến reactive
            timer: null
        };
    },
    computed: {
        timeAgoFormatted() {
            return timeAgo(this.comment.CreatedAt, this.now); // dùng now để reactive
        }
    },
    mounted() {
        // ✅ Cập nhật mỗi 60s
        this.timer = setInterval(() => {
            this.now = new Date();
        }, 60000);
    },
    beforeUnmount() {
        clearInterval(this.timer); // clear khi component bị remove
    }
};
</script>
