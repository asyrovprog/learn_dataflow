# Quiz 07: WriteOnceBlock & Immutable Broadcasting

Answer the following questions to test your understanding of WriteOnceBlock.

**Format**:
- Y/N questions: Answer with Y or N
- Multiple choice (A-D): Answer with the letter
- Multi-select (M:): Answer with multiple letters like "AC" or "BD"

---

## Q1: Single-Assignment Semantics (Y/N)
If three producers simultaneously post values 10, 20, and 30 to a WriteOnceBlock, is it guaranteed that exactly one value will be accepted and the other two will be rejected?

**Answer**: 

---

## Q2: Cloning Behavior (A-D)
What is the correct constructor parameter for the cloning function when creating a WriteOnceBlock?

A) A lambda that clones the object  
B) Always `null`  
C) A function that returns the same instance  
D) Depends on whether you want immutable broadcasting

**Answer**:

---

## Q3: Post Return Value (A-D)
What does the `Post()` method return when called on a WriteOnceBlock that already has a value?

A) Throws InvalidOperationException  
B) Returns `true` and overwrites the value  
C) Returns `false` and the value is rejected  
D) Blocks until the current value is consumed

**Answer**:

---

## Q4: Broadcasting vs Latest-Wins (A-D)
How does WriteOnceBlock differ from BroadcastBlock in terms of accepting values?

A) WriteOnceBlock accepts unlimited values and keeps the latest; BroadcastBlock accepts one  
B) WriteOnceBlock accepts exactly one value; BroadcastBlock accepts unlimited and keeps the latest  
C) Both accept exactly one value  
D) Both accept unlimited values but use different cloning strategies

**Answer**:

---

## Q5: Use Case Selection (M:)
Which scenarios are appropriate use cases for WriteOnceBlock? (Select all that apply)

A) Racing multiple async cache lookups where first result wins  
B) Broadcasting real-time stock price updates  
C) Single initialization of expensive resources  
D) Streaming sensor data to multiple consumers  
E) Broadcasting a start signal to multiple waiting workers

**Answer**:

---

## Q6: Completion Behavior (A-D)
What happens when you call `ReceiveAsync()` on a WriteOnceBlock that has been completed without posting a value?

A) Returns `null`  
B) Blocks indefinitely waiting for a value  
C) Throws InvalidOperationException  
D) Returns a default value for the type

**Answer**:

---

## Q7: Instance Sharing (Y/N)
Does WriteOnceBlock broadcast the same object instance to all connected targets (no cloning)?

**Answer**:

---

## Q8: Race Condition Pattern (A-D)
In a lazy initialization pattern using WriteOnceBlock, multiple threads call an initialization method simultaneously. What ensures only one initialization occurs?

A) The WriteOnceBlock's mutex locks other threads  
B) The first `Post()` succeeds; subsequent posts return false  
C) The TryReceive() method prevents re-initialization  
D) The Complete() method must be called first

**Answer**:

---

## Scoring
- 8/8 correct: 100% ✅ Perfect!
- 7/8 correct: 87.5% ✅ Pass
- 6/8 correct: 75% ⚠️ Borderline (review recommended)
- <6 correct: <75% ❌ Retry recommended
