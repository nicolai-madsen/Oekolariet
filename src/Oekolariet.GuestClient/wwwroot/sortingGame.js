export function enableImmediateDrag(card, onDrop) {
  let drag = null;

  // Let the finger move the card without scrolling or selecting text.
  card.style.touchAction = "none";
  card.style.userSelect = "none";

  card.onpointerdown = event => {
    if (drag || !event.isPrimary || event.button !== 0) return;

    event.preventDefault();
    drag = { id: event.pointerId, x: event.clientX, y: event.clientY };
    card.setPointerCapture(event.pointerId);
  };

  card.onpointermove = event => {
    if (drag?.id !== event.pointerId) return;

    const x = event.clientX - drag.x;
    const y = event.clientY - drag.y;
    card.style.transform = `translate(${x}px, ${y}px)`;
  };

  function resetDrag() {
    drag = null;
    card.style.transform = "";
  }

  card.onpointerup = event => {
    if (drag?.id !== event.pointerId) return;

    // Look underneath the card to find the bin at the release position.
    const bin = document.elementsFromPoint(event.clientX, event.clientY)
      .find(element => element.hasAttribute("data-bin"));

    resetDrag();
    if (bin) onDrop.invokeMethodAsync("HandleBinDrop", bin.dataset.bin);
  };

  // Reset the card when the browser releases pointer capture.
  card.onlostpointercapture = resetDrag;
}
