// Canvas setup
var canvas = document.getElementById("canvas");
var ctx = canvas.getContext("2d");
w = $(canvas).width();
h = $(canvas).height();
// Flip the canvas so the y axis starts bottom left like it does in our
// unity project 
ctx.transform(1, 0, 0, -1, 0, canvas.height);

// Current selected tool
var tool = "line";
var drawing = false; 
var preview = {start: [0, 0], end: [0, 0]}
// Current loaded map
var points = //[];
	[[0, 0], [0.2, 0.2], [0.5, 0.5], [0.5, 0.8]];
var edges = //[];
	[[0, 1], [1, 3]];

pointSize = 4; 
gridSize = 0.05;

// Tool selection
$(".editor-tools button").on("click", function(e) {
	$(".editor-tools button").removeClass("active");
	$(e.target).addClass("active");
	tool = $(e.target).attr("data-tool");
})

// Canvas functionality
startDrawing = function(e) {
	drawing = true; 
	x = numberToGrid(e.offsetX);
	y = numberToGrid(e.offsetY);
	preview.start = [
		transformCanvasToUnityX(x), 
		transformCanvasToUnityY(y)
	]
	$(canvas).on("mousemove", draw);
}
stopDrawing = function(cancel) {
	drawing = false; 
	$(canvas).off("mousemove", draw);
	//
	if (!cancel) 
		addShape();
	render();
}
cancelDrawing = function () {
	stopDrawing(true);
}
// accepts e for mouse events 
draw = function(e) {
	x = numberToGrid(e.offsetX);
	y = numberToGrid(e.offsetY);
	preview.end = [
		transformCanvasToUnityX(x), 
		transformCanvasToUnityY(y)
	]
	render();
}

// Transform X canvas units into values from 0 to 1
transformCanvasToUnityX = function (x) {
	return x / w;
}
// Transform Y canvas units into values from 0 to 1
// We also flip the origin axis
transformCanvasToUnityY = function (y) {
	return (h - y) / h;
}
// Snap to grid. Assumes 1/1 ratio on canvas 
numberToGrid = function (n) {
	snap = w * gridSize;
	return Math.round(n / snap) * snap;
}

// Press down
$(canvas).on("mousedown", startDrawing);
// Press up 
$(canvas).on("mouseup", function(e) {
	//console.log("up", e.offsetX, e.offsetY);
	stopDrawing();
});
// Press up, outside of the canvas 
$(document).on("mouseup", function(e) {
	// Cancel the drawing if we release outside of the canvas
	if ($(e.target)[0] != $(canvas)[0])
		cancelDrawing();
});



// Addition of the currently drawn shape into the list
// Shapes are independent of the grid on purpose
addShape_line = function() {
	distX = preview.end[0] - preview.start[0];
	distY = preview.end[1] - preview.start[1];
	steps = Math.abs(distX) * (w * gridSize) + Math.abs(distY)  * (w * gridSize);
	stepX = distX / steps;
	stepY = distY / steps;
	
	points_connected = points.length;
	
	for (i = 0; i < steps; i++) {
		coordinates = [
			preview.start[0] + stepX * i,
			preview.start[1] + stepY * i
			];
		points.push(coordinates);
	}

	// -1 
	for (i = points_connected; i < points.length - 1; i++) {
		edges.push([i, i+1])
	}
}
addShape_circle = function() {

}
addShape_free = function() {
}
addShape_remove = function() {
}
addShape = function() {
	window["addShape_" + tool]();
	render();
}

// p1 = [x,y], p2 = [x, y]
drawLine = function(p1, p2, color) {
	ctx.beginPath();
	ctx.strokeStyle = color;
	ctx.moveTo(p1[0] * w, p1[1] * h);
	ctx.lineTo(p2[0] * w, p2[1] * h);
	ctx.stroke();
}

// .. 
loadLevel = function(level) {
	//...
	render();
}

// Render previews
renderPreview_line = function() {
	drawLine(preview.start, preview.end, "green");
}
renderPreview_circle = function() {
}
renderPreview_free = function() {
}
renderPreview_remove = function() {
}

render = function() {
	// Clear the canvas
	ctx.clearRect(0, 0, canvas.width, canvas.height);

	// Render grid
	for (i = 0; i < 1 / gridSize; i++) {
		drawLine([gridSize * i, 0], [gridSize * i, 1], "grey");
		drawLine([0, gridSize * i], [1, gridSize * i], "grey");
	}

	// Render points 
	for (i = 0; i < points.length; i++) {
		ctx.beginPath();
		ctx.strokeStyle = "blue";
		ctx.arc(
			(points[i][0] * w), 
			(points[i][1] * h), 
			pointSize, 
			0, 
			2 * Math.PI);
		ctx.stroke();
	}
	// Render edges
	for (i = 0; i < edges.length; i++) {
		// Create the lines between the relevant points:
		// points[ what point? ] [x or y as in 0 or 1]
		// The point is figured within edges[looped i] [ first point 0 or second point 1 ]
		drawLine(
			[points[edges[i][0]][0], points[edges[i][0]][1]],
			[points[edges[i][1]][0], points[edges[i][1]][1]],
			"red"
		);
	}
	// Render preview 
	if (drawing)
		window["renderPreview_" + tool]()
}
loadLevel();


scaleToCanvas = function(s) {

	return 
}