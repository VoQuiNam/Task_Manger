<template>
  <div class="login-container">
    <img src="https://seeklogo.com/images/A/atlassian-logo-F3EDDBE4E0-seeklogo.com.png" alt="Atlassian Logo" width="80"
      class="logo">
    <h4 class="mb-3">Log in to continue</h4>


    <form>
      <div class="mb-3">
        <input type="email" class="form-control" placeholder="Enter your email" required>
      </div>

      <div class="form-check mb-3">
        <input type="checkbox" class="form-check-input" id="rememberMe">
        <label class="form-check-label" for="rememberMe">Remember me</label>
      </div>
      <button type="submit" class="btn btn-primary w-100">Continue</button>
    </form>

    <p class="mt-3">Or log in with:</p>
    <div class="d-flex justify-content-center gap-2">
      <button class="btn btn-outline-danger w-50">Google</button>
      <button class="btn btn-outline-primary w-50">Facebook</button>
    </div>

    <p class="mt-4">
      <a href="#" class="text-decoration-none">Can't log in?</a> ·
      <a href="#" class="text-decoration-none">Create an account</a>
    </p>
    <p class="text-muted small">One account for Jira, Confluence, Trello, and more.</p>
  </div>
</template>

<script>
  export default {
    name: 'LoginPage',
  };
</script>

<style>
  body {
    display: flex;
    justify-content: center;
    align-items: center;
    height: 100vh;
    background-color: #f4f5f7;
  }

  .login-container {
    background: white;
    padding: 2rem;
    border-radius: 10px;
    box-shadow: 0 4px 10px rgba(0, 0, 0, 0.1);
    width: 100%;
    max-width: 400px;
    text-align: center;
  }

  .logo {
    margin-bottom: 1rem;
  }

  .form-check-label {
    float: left;
  }
</style>