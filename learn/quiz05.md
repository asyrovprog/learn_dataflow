# Quiz 05: JoinBlock & Coordinated Data Flows

**Topic:** JoinBlock & Coordinated Data Flows in TPL Dataflow  
**Questions:** 8  
**Format:** Y/N, A-D, Multi-select (M:)  
**Passing Score:** 80% (7/8 correct)

---

## Question 1 (Y/N)
JoinBlock waits for items to arrive at ALL input targets before producing a paired tuple output.

**Your Answer:** ____

---

## Question 2 (A-D)
What happens in **greedy mode** when JoinBlock receives items at different rates across inputs?

A) Items are immediately rejected if not all inputs have data  
B) Items are consumed immediately and buffered until all inputs have matching items  
C) Processing stops until all inputs receive items simultaneously  
D) Only the fastest input is processed, others are ignored  

**Your Answer:** ____

---

## Question 3 (A-D)
In the following code, what will be the first tuple output?

```csharp
var join = new JoinBlock<int, string>();
join.Target1.Post(10);
join.Target1.Post(20);
join.Target2.Post("A");
join.Target2.Post("B");
```

A) (10, "B")  
B) (20, "A")  
C) (10, "A")  
D) (20, "B")  

**Your Answer:** ____

---

## Question 4 (Y/N)
Non-greedy mode in JoinBlock prevents unbounded buffering by only consuming items when ALL inputs have data available.

**Your Answer:** ____

---

## Question 5 (A-D)
What is the maximum number of inputs that JoinBlock supports out-of-the-box?

A) 3 inputs  
B) 5 inputs  
C) 7 inputs  
D) Unlimited inputs  

**Your Answer:** ____

---

## Question 6 (M:)
Which of the following are common patterns for using JoinBlock? (Select all that apply)

A) ID-entity lookup (pairing IDs with fetched data)  
B) Multi-source aggregation (combining parallel results)  
C) Synchronized fan-in (merging multiple processing paths)  
D) Single-input transformation (like TransformBlock)  
E) Background task scheduling  

**Your Answer:** M: ____

---

## Question 7 (A-D)
When does JoinBlock complete its execution?

A) When the first input target is completed  
B) When any input target receives an item  
C) When ALL input targets are completed AND all buffered items are paired and delivered  
D) When the output buffer reaches capacity  

**Your Answer:** ____

---

## Question 8 (A-D)
In this pipeline, what configuration would prevent memory buildup if `orderSource` posts much faster than `inventoryCheck` processes?

```csharp
var join = new JoinBlock<Order, InventoryStatus>();
orderSource.LinkTo(join.Target1);
inventoryCheck.LinkTo(join.Target2);
```

A) Set `BoundedCapacity = 10` on JoinBlock  
B) Set `Greedy = false` on JoinBlock  
C) Set `MaxMessagesPerTask = 1` on JoinBlock  
D) Use `Complete()` on orderSource immediately  

**Your Answer:** ____

---

**Submit your answers in format: 1:Y 2:B 3:C 4:N 5:A 6:M:A,B,C 7:C 8:B**