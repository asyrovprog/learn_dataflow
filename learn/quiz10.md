# Quiz 10: Custom IPropagatorBlock Implementation

**Instructions:** Answer each question using the specified format. Submit all answers in one line like: `1:A,2:BC,3:Y,4:C,5:ABD,6:N,7:D,8:ABCD`

---

## Q1. What interfaces must a class implement to be a full IPropagatorBlock<TIn, TOut>? (Multi-select: M:)

A) ITargetBlock<TIn>  
B) ISourceBlock<TOut>  
C) IDataflowBlock  
D) IPropagatorBlock<TIn, TOut> (directly)  
E) IObservable<TOut>

---

## Q2. When should OfferMessage return DataflowMessageStatus.Declined? (Single: A-D)

A) When the block has completed and won't accept more messages  
B) When buffer is temporarily full but will drain over time  
C) When the block needs to coordinate with other sources first  
D) When the source no longer has the message available

---

## Q3. What's the key difference between Declined and Postponed return values? (Single: A-D)

A) Declined means permanent rejection; Postponed means temporary  
B) Declined triggers source retry (push); Postponed triggers target pull (reserve/consume)  
C) Declined is for greedy blocks; Postponed is only for completion  
D) Declined is thread-safe; Postponed requires locking

---

## Q4. Why does the reservation protocol require BOTH ReserveMessage AND ConsumeMessage? (Single: A-D)

A) To improve performance by splitting the operation into two phases  
B) To enable atomic multi-source coordination (all-or-nothing message acquisition)  
C) To allow multiple targets to share the same message  
D) To implement thread-safe locking without mutexes

---

## Q5. In OfferMessage, when consumeToAccept=true, what must the target do? (Single: A-D)

A) Use the messageValue parameter directly and store it  
B) Call source.ConsumeMessage to complete the two-phase protocol  
C) Return Postponed and reserve the message later  
D) Check if the source is null before accepting

---

## Q6. Is there a built-in timeout/expiration for reserved messages in TPL Dataflow? (Y/N)

---

## Q7. Which statements about the simple PropagateMessages() implementation are TRUE? (Multi-select: M:)

A) It has no retry mechanism if all targets decline  
B) Messages can get stuck in buffer until new activity triggers propagation  
C) It's suitable for production use with bounded capacity targets  
D) Real production blocks use async loops or timers for continuous retry  
E) It demonstrates the protocol but needs enhancement for reliability

---

## Q8. What's the main risk of holding a lock during async operations in custom blocks? (Single: A-D)

A) Performance degradation due to thread contention  
B) Potential deadlock if async operation tries to acquire the same lock  
C) Memory leaks from unreleased lock objects  
D) Race conditions between async continuations

---

## Q9. Which scenarios require building custom IPropagatorBlock instead of using Encapsulate? (Multi-select: M:)

A) Combining existing blocks in a standard pipeline  
B) Non-standard buffering logic (e.g., priority queue, LRU cache)  
C) Custom reservation protocol implementation  
D) Quick prototyping of dataflow patterns  
E) Integration with non-dataflow legacy systems

---

## Q10. When implementing Complete(), what must happen before transitioning Completion task to RanToCompletion? (Single: A-D)

A) All linked targets must complete first  
B) Set _decliningPermanently=true immediately  
C) Output buffer must be drained (all messages propagated)  
D) Cancel all pending async operations

---

**Submit your answers in format:** `1:ABC,2:B,3:Y,4:D,5:BCE,6:N,7:AD,8:B,9:ACE,10:C`
