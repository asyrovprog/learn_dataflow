# Quiz 05 Answer Key: JoinBlock & Coordinated Data Flows

**Correct Answers:** 1:Y 2:B 3:C 4:Y 5:C 6:M:A,B,C 7:C 8:B

---

## Question 1: Y
**Answer:** Y (Yes)  
**Reasoning:** This is the fundamental behavior of JoinBlock. It synchronizes multiple input streams by waiting until it receives at least one item from ALL input targets before producing a matched tuple. This "zip" behavior is what makes JoinBlock useful for coordinating parallel data sources.

## Question 2: B
**Answer:** B - Items are consumed immediately and buffered until all inputs have matching items  
**Reasoning:** In greedy mode (the default), JoinBlock immediately consumes and stores items as they arrive at each input target. It holds them in internal buffers until it has items from all inputs to create a complete tuple. This can lead to unbounded buffering if inputs arrive at very different rates.

## Question 3: C
**Answer:** C - (10, "A")  
**Reasoning:** JoinBlock maintains FIFO (First-In-First-Out) order per input. The first item posted to Target1 was 10, and the first item posted to Target2 was "A", so they get paired together in the first output tuple.

## Question 4: Y
**Answer:** Y (Yes)  
**Reasoning:** Non-greedy mode prevents unbounded buffering by using a 2-phase protocol. It only consumes items when ALL inputs have items available, using reservations to ensure atomic consumption. This prevents memory buildup when sources have unpredictable posting rates.

## Question 5: C
**Answer:** C - 7 inputs  
**Reasoning:** TPL Dataflow provides JoinBlock overloads for 2 through 7 inputs out-of-the-box. For more than 7 inputs, you would need to chain multiple JoinBlocks or create custom blocks.

## Question 6: A,B,C
**Answer:** M: A,B,C  
**Reasoning:** 
- A) ID-entity lookup: Pairing IDs with corresponding data fetched from databases
- B) Multi-source aggregation: Combining results from parallel operations  
- C) Synchronized fan-in: Merging multiple processing paths that must complete together
- D) Single-input transformation is handled by TransformBlock, not JoinBlock
- E) Background task scheduling is not a JoinBlock pattern

## Question 7: C
**Answer:** C - When ALL input targets are completed AND all buffered items are paired and delivered  
**Reasoning:** JoinBlock completes only when two conditions are met: (1) all input targets have been completed via Complete(), and (2) all internally buffered items have been successfully paired and delivered to consumers.

## Question 8: B
**Answer:** B - Set `Greedy = false` on JoinBlock  
**Reasoning:** Non-greedy mode prevents the fast-posting orderSource from building up a large buffer in JoinBlock's Target1. Instead, it will only consume orders when the slower inventoryCheck has results available, naturally throttling the pipeline to the speed of the slowest component.