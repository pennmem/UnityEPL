using System;

namespace UnityEPL {

    // If "i" goes past "limit", an exception is thrown with the stored message.
    public class BoundedInt {
        private int limit;
        private string message;
        private int i_;
        public int i {
            get { Assert(i_); return i_; }
            set { i_ = value; }
        }

        public BoundedInt(int limit_, string message_) {
            limit = limit_;
            message = message_;
        }

        private void Assert(int i) {
            if (i >= limit) {
                throw new IndexOutOfRangeException(message);
            }
        }
    }

}
