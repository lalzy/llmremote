// ==== Status ====

const checkSeconds = 1000;
async function setLlamaStatus(){
    const llamaStatus = document.getElementById("llamastatus");
    
    const res = await fetch(`/api/llama/health`);
    const status = await res.json();
    llamaStatus.textContent = `Llama: ${status}`;
    llamaStatus.className = `server-status ${status.toLowerCase()}`;
    switch(status.toLowerCase()){
    case "online":
	setItemState("running");
	return;
    case "loading":
	setItemState("loading");
	return;
    default:
	setItemState(null);
    }
}

setLlamaStatus();
setInterval(setLlamaStatus, checkSeconds);


// ==== Model List ===
const modelList = document.getElementById("modelList");
const emptyText = document.getElementById("emptyText");

// === LLamaModelState ===
let activeId = localStorage.getItem("activeId");

function setItemState(state){
    // Clear old states
    document.querySelectorAll("#modelList li").forEach(li => li.classList.remove("loading", "running"));

    if (activeId === null || !state) return;
    const item = document.querySelector(`#modelList li[data-id="${activeId}"]`);
    if(item) item.classList.add(state);
}


function addItemButton(buttonText, func){
    const button = document.createElement("button");
    button.textContent = buttonText;
    button.addEventListener("click", (event) => {
	event.stopPropagation();
	func();
    });
    return button;
}

// Create the list of models
function createModelItems(models){
    for(const model of models){
	const item = document.createElement("li");
	item.textContent = `${model.name}`;
	item.dataset.id = model.id;

	item.addEventListener("click", async () => {
	    activeId = model.id;
	    localStorage.setItem("activeId", model.id);
	    setItemState("loading");
	    
	    // Start the llama process
	    const response = await fetch(`/api/llama/start`, {
		method:"POST",
		headers: {"Content-Type":"application/json"},
		body: JSON.stringify(model)
	    });
	});

	const editButton = addItemButton("✎", () => openEdit(model));
	const deleteButton = addItemButton("✕", () => openDelete(model));
	deleteButton.classList.add("delete-button");
	item.append(editButton, deleteButton)
	modelList.appendChild(item);
    }
}

// Change sort order
const sortSelect = document.getElementById("sortSelect");
sortSelect.value = localStorage.getItem("orderBy") ?? "Name";
sortSelect.addEventListener("change", () => {
    localStorage.setItem("orderBy", sortSelect.value);
    loadModels();
});

async function loadModels(){
    const response = await fetch(`/api/llmmodel/all?orderBy=${sortSelect.value}`);
    
    if (!response.ok) {
        console.error("Load failed:", response.status, await response.text());
        return;
    }
    
    modelList.innerHTML = "";
    
    const models = await response.json();

    createModelItems(models)
    
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

function openEdit(model){
    editingId = model.id;
    modelDialogTitle.textContent = "Edit Model";
    document.getElementById("nameInput").value = model.name;
    document.getElementById("filePathInput").value = model.filePath;
    document.getElementById("contextInput").value = model.context ?? "";
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

function openDelete(model){
    deletingId = model.id;
    deleteName.textContent = model.name;
    deleteDialog.showModal();
}

document.getElementById("confirm-button").addEventListener("click", async () => {
    const response = await fetch(`api/llmmodel/${deletingId}`, {method: "DELETE"});
    if(!response.ok){
	console.Error("Delete failed:", response.status, await response.text());
	return;
    }

    deleteDialog.close();
    loadModels();
});

// ==== Submit ====

const modelForm = document.getElementById("modelForm");
const formError = document.getElementById("formError");

modelForm.addEventListener("submit", async (event) => {
    event.preventDefault();

    const model = {
	Name: document.getElementById("nameInput").value,
	FilePath: document.getElementById("filePathInput").value,
	Context: document.getElementById("contextInput").value || null,
    };
    
    const url = editingId === null ? "/api/llmmodel/add" : `/api/llmmodel/${editingId}`;
    const method = editingId === null ? "POST" : "PUT";
    
    const response = await fetch(url, {
	method:method,
	headers: {"Content-Type":"application/json"},
	body: JSON.stringify(model)
    });

    if(!response.ok){
	formError.textContent = "Could not save the model.";
	return;
    }

    formError.textContent = "";
    modelForm.reset();
    modalDialog.close();
    loadModels();
});
