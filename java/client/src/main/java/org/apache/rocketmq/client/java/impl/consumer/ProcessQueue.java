/*
 * Licensed to the Apache Software Foundation (ASF) under one or more
 * contributor license agreements.  See the NOTICE file distributed with
 * this work for additional information regarding copyright ownership.
 * The ASF licenses this file to You under the Apache License, Version 2.0
 * (the "License"); you may not use this file except in compliance with
 * the License.  You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

package org.apache.rocketmq.client.java.impl.consumer;

import org.apache.rocketmq.client.java.route.MessageQueueImpl;

public interface ProcessQueue {
    /**
     * Get the mapped message queue.
     *
     * @return mapped message queue.
     */
    MessageQueueImpl getMessageQueue();

    /**
     * Drop the current process queue, which means the process queue's lifecycle is over,
     * thus it would not fetch messages from the remote anymore if dropped.
     */
    void drop();

    /**
     * {@link PushProcessQueue} would be regarded as expired if no fetch message for a long time.
     *
     * @return if it is expired.
     */
    boolean expired();

    /**
     * Get the count of cached messages.
     *
     * @return count of pending messages.
     */
    long getCachedMessageCount();

    /**
     * Get the bytes of cached message memory footprint.
     *
     * @return bytes of cached message memory footprint.
     */
    long getCachedMessageBytes();

    /**
     * Do some stats work.
     */
    void doStats();
}
