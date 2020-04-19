<template>
    <div id="app" class="wrapper">
        <navigation/>
        <router-view />
        <footer-vue/>
    </div>
</template>

<script lang="ts">
    import { Component, Vue } from 'vue-property-decorator';
    import Home from "./components/home.vue";
    import Navigation from "./components/navigation.vue";
    import FooterVue from "./components/footer.vue";
    
    @Component({
        components: {
            Navigation,
            Home,
            FooterVue
        }
    })
    export default class App extends Vue { }
</script>

<style lang="sass">
    html 
        scroll-behavior: smooth 

    $body-font-size: 1.2em !important
    
    @import "~bulma/bulma"

    .has-image-centered 
        margin-left: auto
        margin-right: auto
</style>
