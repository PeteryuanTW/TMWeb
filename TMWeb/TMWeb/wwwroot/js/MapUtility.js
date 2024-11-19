function GetPosition(element) {
    const rect = element.getBoundingClientRect();
    return {
        x: rect.x,
        y:rect.y,
        top: rect.top,
        left: rect.left,
        height: rect.height,
        width: rect.width,
    };
}