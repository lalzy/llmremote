// ==== Status ====

const checkMS = 1000;
async function setLlamaStatus() {
	const llamaStatus = document.getElementById("llamastatus");

	const res = await fetch(`/api/llama/health`);
	const status = await res.json();
	llamaStatus.innerHTML = `Llama: <span>${status}</span>`;
	llamaStatus.className = `server-status ${status.toLowerCase()}`;
	switch (status.toLowerCase()) {
		case "online": {
			const working = await (await fetch(`/api/llama/working`)).json();
			setItemState(working ? "working" : "running");
			return;
		} case "loading":
			setItemState("loading");
			return;
		default:
			setItemState(null);
	}
}

setLlamaStatus();
setInterval(setLlamaStatus, checkMS);

// ==== Model List ===
const modelList = document.getElementById("modelList");
const emptyText = document.getElementById("emptyText");


// ==== Folders ===

function createFolder(name, models) {
	const folder = document.createElement("li");
	folder.className = "folder";
	folder.innerHTML = `<details><summary></summary><ul></ul></details>`;
	folder.querySelector("summary").textContent = name;

	const summary = folder.querySelector("summary");
	summary.textContent = name;
	const addButton = addItemButton("+", () => openFolderDialog(name, models));
	addButton.addEventListener("click", (event) => event.preventDefault()); // don't toggle the folder
	summary.append(addButton);

	folder.addEventListener("dragover", (event) => event.preventDefault());
	folder.addEventListener("drop", async (event) => {
		event.preventDefault();
		const model = JSON.parse(event.dataTransfer.getData("application/json"));
		if (model.category === name) return;
		await saveModel(model.id, { ...model, category: name });
		loadModels();
	});

	const details = folder.querySelector("details");
	details.open = localStorage.getItem(`folder:${name}`) === "open";
	details.addEventListener("toggle", () => {
		localStorage.setItem(`folder:${name}`, details.open ? "open" : "closed");
	});

	return folder;
}

const emptyFolders = new Set();

document.getElementById("addCategory").addEventListener("click", () => {
	const name = prompt("Folder name")?.trim();
	if (!name) return;
	emptyFolders.add(name);
	loadModels();
});

const folderDialog = document.getElementById("folderDialog");
const folderDialogName = document.getElementById("folderDialogName");
const folderModelSelect = document.getElementById("folderModelSelect");
let addingToFolder = null;

function openFolderDialog(name, models) {
	addingToFolder = { name, models };
	folderDialogName.textContent = name;
	folderModelSelect.replaceChildren(
		...models.filter(m => m.category !== name).map(m => new Option(m.name, m.id)));
	folderDialog.showModal();
}

document.getElementById("folderForm").addEventListener("submit", async (event) => {
	event.preventDefault();
	const model = addingToFolder.models.find(m => m.id === folderModelSelect.value);
	if (!model) return;
	await saveModel(model.id, { ...model, category: addingToFolder.name });
	folderDialog.close();
	loadModels();
});

// === LLamaModelState ===
let activeId = localStorage.getItem("activeId");

function setItemState(state) {
	// Clear old states
	document.querySelectorAll("#modelList li").forEach(li => li.classList.remove("loading", "running", "working"));
	if (activeId === null || !state) return;
	const item = document.querySelector(`#modelList li[data-id="${activeId}"]`);
	if (item) item.classList.add(state);
}


function addItemButton(buttonText, func) {
	const button = document.createElement("button");
	button.textContent = buttonText;
	button.addEventListener("click", (event) => {
		event.stopPropagation();
		func();
	});
	return button;
}

async function stopModel(modelId) {
	if (modelId == activeId) {
		// Stop the llama process
		await fetch(`/api/llama/stop`, { method: "POST" });

		// State cleanup
		activeId = null;
		localStorage.removeItem("activeId");
		setItemState("stopped");
		return true;
	}
	return false;
}

async function startModel(model) {
	activeId = model.id;
	localStorage.setItem("activeId", model.id);
	setItemState("loading");

	// Start the llama process
	const response = await fetch(`/api/llama/start`, {
		method: "POST",
		headers: { "Content-Type": "application/json" },
		body: JSON.stringify(model)
	});
}



// Create the list of models
function createModelItem(model) {
	const item = document.createElement("li");
	item.textContent = `${model.name}`;
	item.dataset.id = model.id;
	item.draggable = true;

	item.addEventListener("click", async () => {
		if (await stopModel(model.id)) return;
		await startModel(model);
	});
	item.addEventListener("dragstart", (event) => {
		event.dataTransfer.setData("application/json", JSON.stringify(model));
	});

	const starButton = addItemButton(model.isFavorite ? "★" : "☆", async () => {
		await saveModel(model.id, { ...model, isFavorite: !model.isFavorite });
		loadModels();
	});
	const editButton = addItemButton("✎", () => openEdit(model));
	const deleteButton = addItemButton("✕", () => openDelete(model));
	deleteButton.classList.add("delete-button");
	item.append(starButton, editButton, deleteButton);
	return item;
}

function placeInFolders(models) {
	const folders = new Map();
	for (const name of emptyFolders) folders.set(name, createFolder(name, models));
	const loose = [];

	for (const model of models) {
		const item = createModelItem(model);
		if (!model.category) {
			loose.push(item);
			continue;
		}
		if (!folders.has(model.category)) folders.set(model.category, createFolder(model.category, models));
		folders.get(model.category).querySelector("ul").appendChild(item);
	}
	modelList.append(...folders.values(), ...loose);
}

// Change sort order
const sortSelect = document.getElementById("sortSelect");
sortSelect.value = localStorage.getItem("orderBy") ?? "Name";
sortSelect.addEventListener("change", () => {
	localStorage.setItem("orderBy", sortSelect.value);
	loadModels();
});

async function loadModels() {
	const response = await fetch(`/api/llmmodel/all?orderBy=${sortSelect.value}`);

	if (!response.ok) {
		console.error("Load failed:", response.status, await response.text());
		return;
	}

	modelList.innerHTML = "";

	const models = await response.json();

	placeInFolders(models.toSorted((a, b) => b.isFavorite - a.isFavorite));

	emptyText.hidden = models.length > 0;
}

loadModels();

// ==== Add/Edit modal ====
const addModelButton = document.getElementById("addModel");
const modalDialog = document.getElementById("modalDialog");
let editingId = null;
const modelDialogTitle = document.getElementById("modelDialogTitle");

addModelButton.addEventListener("click", () => {
	editingId = null;
	modelDialogTitle.textContent = "Add Model";
	modelForm.reset();
	modalDialog.showModal();
});

function openEdit(model) {
	editingId = model.id;
	modelDialogTitle.textContent = "Edit Model";
	document.getElementById("nameInput").value = model.name;
	document.getElementById("filePathInput").value = model.filePath;
	document.getElementById("contextInput").value = model.context ?? "";
	document.getElementById("argumentInput").value = model.arguments ?? "";
	modalDialog.showModal();
}

// ==== Cancel Button ====
document.querySelectorAll(".cancel-button").forEach(button => {

	button.addEventListener("click", () => {
		button.closest("dialog").close();
	});
});

// ==== Delete ====
const deleteDialog = document.getElementById("deleteDialog");
const deleteName = document.getElementById("deleteName");
let deletingId = null;

function openDelete(model) {
	deletingId = model.id;
	deleteName.textContent = model.name;
	deleteDialog.showModal();
}

document.getElementById("confirm-button").addEventListener("click", async () => {
	const response = await fetch(`api/llmmodel/${deletingId}`, { method: "DELETE" });
	if (!response.ok) {
		console.Error("Delete failed:", response.status, await response.text());
		return;
	}

	deleteDialog.close();
	loadModels();
});

// ==== Submit ====

const modelForm = document.getElementById("modelForm");
const formError = document.getElementById("formError");

async function saveModel(id, model) {
	const url = id === null ? "/api/llmmodel/add" : `/api/llmmodel/${id}`;
	const method = id === null ? "POST" : "PUT";

	return await fetch(url, {
		method: method,
		headers: { "Content-Type": "application/json" },
		body: JSON.stringify(model)
	});
}

modelForm.addEventListener("submit", async (event) => {
	event.preventDefault();

	const model = {
		Name: document.getElementById("nameInput").value,
		FilePath: document.getElementById("filePathInput").value,
		Context: document.getElementById("contextInput").value || null,
		Arguments: document.getElementById("argumentInput").value || null,
	};

	const response = await saveModel(editingId, model);

	if (!response.ok) {
		formError.textContent = "Could not save the model.";
		return;
	}

	formError.textContent = "";
	modelForm.reset();
	modalDialog.close();
	loadModels();
});


// === Timer ===

const timerLeft = document.getElementById("timerLeft");
const timerCancel = document.getElementById("timerCancel");

async function updateTimer() {
	const res = await fetch("/api/timer/shutdowntime");
	const data = res.status === 204 ? null : await res.json();

	timerLeft.parentElement.hidden = false;
	if (data == null) {
		timerLeft.textContent = "Not-started";
		timerCancel.hidden = true;
		return;
	}

	const secondsLeft = Math.max(0, Math.floor((new Date(data) - Date.now()) / 1000));
	const hours = Math.floor(secondsLeft / 3600);
	const minutes = Math.floor((secondsLeft % 3600) / 60);
	const seconds = secondsLeft % 60;
	timerLeft.textContent = `${hours}:${String(minutes).padStart(2, "0")}:${String(seconds).padStart(2, "0")}`;
	timerCancel.hidden = false;
}

// Set Button
const timerUnit = document.getElementById("timerUnit");
const timerValue = document.getElementById("timerValue");
const timerSet = document.getElementById("timerSet");
const unitSeconds = { Minutes: 60, Hours: 3600 };

timerSet.addEventListener("click", async () => {
	const seconds = Math.floor(Number(timerValue.value) * unitSeconds[timerUnit.value]);
	if (seconds < 60) {
		timerError.textContent = "Minimum is 1 minute";
		return;
	};

	await fetch(`/api/timer/set?duration=${seconds}`, {
		method: "POST",
		headers: { "Content-Type": "application/json" },
	});
	updateTimer();
});

timerCancel.addEventListener("click", async () => {
	await fetch("/api/timer/cancel", { method: "POST" });
	updateTimer();
});

updateTimer();
setInterval(updateTimer, checkMS);

// === ComfyUI ===
// === ComfyUI ===
const comfyStatus = document.getElementById("comfystatus");
const comfyButton = document.getElementById("comfyButton");
let comfyStarting = false;

async function setComfyStatus() {
	try {
		const res = await fetch("/api/comfy/health");
		const status = await res.json();
		if (status.toLowerCase() !== "offline") comfyStarting = false;
		comfyStatus.innerHTML = `ComfyUI: <span>${comfyStarting ? "Starting" : status}</span>`;
		comfyStatus.className = `server-status ${status.toLowerCase()}`;
		comfyButton.textContent = status.toLowerCase() === "offline" && !comfyStarting ? "Start" : "Stop";
	} catch {}
}

comfyButton.addEventListener("click", async () => {
	const starting = comfyButton.textContent === "Start";
	comfyStarting = starting;
	await fetch(starting ? "/api/comfy/start" : "/api/comfy/stop", { method: "POST" });
	setComfyStatus();
});

setComfyStatus();
setInterval(setComfyStatus, checkMS);

// === Nvidia HW Usage ===
const gpuList = document.getElementById("gpus");

async function setGpuUsage() {
	try {
		const res = await fetch("/api/nvidia/usage");
		const gpus = await res.json();

		gpuList.innerHTML = "";
		for (const [name, usage] of Object.entries(gpus)) {
			const card = document.createElement("div");
			card.className = "gpu-card";
			card.innerHTML = `
	<span class="gpu-name">${name}</span>
	<div class="gpu-bar"><div style="width:${usage.gpu}%"></div></div>
	<span class="gpu-stat">${usage.gpu}%</span>
	<span class="gpu-stat">${usage.memory} MiB / ${usage.memoryTotal} MiB</span>`;
			gpuList.appendChild(card);
		}


	} catch {}
}

setGpuUsage();
setInterval(setGpuUsage, checkMS);

// === Voicebox ===
const voiceboxStatus = document.getElementById("voiceboxstatus");
const voiceboxButton = document.getElementById("voiceboxButton");
let voiceboxStarting = false;

async function setVoiceboxStatus() {
	try {
		const res = await fetch("/api/voicebox/health");
		const status = await res.json();
		if (status.toLowerCase() !== "offline") voiceboxStarting = false;
		voiceboxStatus.innerHTML = `Voicebox: <span>${voiceboxStarting ? "Starting" : status}</span>`;
		voiceboxStatus.className = `server-status ${status.toLowerCase()}`;
		voiceboxButton.textContent = status.toLowerCase() === "offline" && !voiceboxStarting ? "Start" : "Stop";
	} catch {}
}

voiceboxButton.addEventListener("click", async () => {
	const starting = voiceboxButton.textContent === "Start";
	voiceboxStarting = starting;
	await fetch(starting ? "/api/voicebox/start" : "/api/voicebox/stop", { method: "POST" });
	setVoiceboxStatus();
});

setVoiceboxStatus();
setInterval(setVoiceboxStatus, checkMS);
