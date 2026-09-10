import { expect, test } from "@playwright/test"

test("cria uma task, move de status e depois exclui", async ({ page }) => {
  const taskTitle = `E2E task ${Date.now()}`

  await page.goto("/")

  await expect(page.getByRole("heading", { name: "TaskFlow" })).toBeVisible()
  await expect(page.getByRole("heading", { name: "To do" })).toBeVisible()
  await expect(page.getByRole("heading", { name: "In progress" })).toBeVisible()
  await expect(page.getByRole("heading", { name: "Done" })).toBeVisible()

  await test.step("cria a task na coluna To do", async () => {
    await page.getByRole("button", { name: "Adicionar task" }).click()
    await page.getByPlaceholder("Título da task").fill(taskTitle)
    await page.getByPlaceholder("Descrição (opcional)").fill("Criada pelo teste E2E")
    await page.getByRole("button", { name: "Criar" }).click()

    await expect(page.getByRole("heading", { name: taskTitle })).toBeVisible()
  })

  const card = page.getByRole("listitem").filter({ hasText: taskTitle })

  await test.step("muda o status para In progress e o card migra de coluna", async () => {
    await card.getByRole("combobox", { name: "Status da task" }).selectOption("IN_PROGRESS")

    await expect(card.getByRole("combobox", { name: "Status da task" })).toHaveValue("IN_PROGRESS")
  })

  await test.step("exclui a task", async () => {
    await card.getByRole("button", { name: `Excluir "${taskTitle}"` }).click({ force: true })

    await expect(page.getByRole("heading", { name: taskTitle })).toHaveCount(0)
  })
})

test("mostra erro de validação ao tentar criar task sem título", async ({ page }) => {
  await page.goto("/")

  await page.getByRole("button", { name: "Adicionar task" }).click()
  await page.getByRole("button", { name: "Criar" }).click()

  await expect(page.getByText("Título é obrigatório.")).toBeVisible()
})
